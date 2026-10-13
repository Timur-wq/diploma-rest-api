using Diploma.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Diploma.Core.Class1;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Diploma.Application.Services;
using Microsoft.EntityFrameworkCore;
using Diploma.DataAccess;
using Diploma.Core.Enums;
using System.Text.Json;
using Diploma.DataAccess.Entities;
using Npgsql;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ServiceEngineer")]  // 🔐 Только для инженеров
    public class EngineerTaskController : ControllerBase
    {
        private readonly IEngineerTaskService _taskService;
        private readonly ILogger<EngineerTaskController> _logger;
        private readonly IDiagnosticActService _diagnosticActService;
        private readonly DiplomaDbContext _context;  // 🔥 Добавлено
        private readonly IPdfGenerationService _pdfService;  // 🔥 Добавлено

        public EngineerTaskController(
            IDiagnosticActService diagnosticActService,
            IEngineerTaskService taskService,
            ILogger<EngineerTaskController> logger,
            DiplomaDbContext context,  // 🔥 Добавлено
            IPdfGenerationService pdfService)  // 🔥 Добавлено
        {
            _taskService = taskService;
            _diagnosticActService = diagnosticActService;
            _logger = logger;
            _context = context;
            _pdfService = pdfService;
        }

        // 🔍 Получить табельный номер из JWT-токена
        private int GetCurrentUserTabNumber()
        {
            // 🔍 Пробуем несколько вариантов claim для TabNum
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirst("sub");

            if (claim == null || !int.TryParse(claim.Value, out var tabNumber))
            {
                _logger.LogError("❌ Не удалось найти TabNum в токене. Claims: {@Claims}",
                    User.Claims.Select(c => new { c.Type, c.Value }));
                throw new UnauthorizedAccessException("Не удалось определить пользователя");
            }

            _logger.LogInformation("✅ Текущий пользователь: TabNum={TabNum}, Login={Login}",
                tabNumber, User.Identity?.Name);

            return tabNumber;
        }

        /// <summary>
        /// Получить все наряды текущего инженера
        /// </summary>
        [HttpGet("my")]
        [ProducesResponseType(typeof(List<EngineerTaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<EngineerTaskDto>>> GetAllMyTasks()
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                _logger.LogInformation("📋 Запрос всех нарядов для сотрудника #{TabNum}", tabNumber);

                var tasks = await _taskService.GetAllTasksAsync(tabNumber);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "❌ Ошибка авторизации");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении нарядов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить активные наряды (в работе или назначенные)
        /// </summary>
        [HttpGet("my/active")]
        [ProducesResponseType(typeof(List<EngineerTaskDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EngineerTaskDto>>> GetActiveTasks()
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                var tasks = await _taskService.GetActiveTasksAsync(tabNumber);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получить завершённые наряды
        /// </summary>
        [HttpGet("my/completed")]
        [ProducesResponseType(typeof(List<EngineerTaskDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<EngineerTaskDto>>> GetCompletedTasks()
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                var tasks = await _taskService.GetCompletedTasksAsync(tabNumber);
                return Ok(tasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получить статистику инженера
        /// </summary>
        [HttpGet("my/stats")]
        [ProducesResponseType(typeof(EngineerStatsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<EngineerStatsDto>> GetMyStats()
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                var stats = await _taskService.GetStatsAsync(tabNumber);
                return Ok(stats);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получить конкретный наряд по ID
        /// </summary>
        [HttpGet("my/{taskId}")]
        [ProducesResponseType(typeof(EngineerTaskDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EngineerTaskDto>> GetTaskById(int taskId)
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                var task = await _taskService.GetTaskByIdAsync(taskId, tabNumber);

                if (task == null)
                    return NotFound(new { message = "Наряд не найден" });

                return Ok(task);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Начать работу по наряду
        /// </summary>
        [HttpPost("my/{taskId}/start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> StartTask(int taskId)
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                _logger.LogInformation("🚀 Сотрудник #{TabNum} начинает наряд #{TaskId}", tabNumber, taskId);

                var success = await _taskService.StartTaskAsync(taskId, tabNumber);

                if (!success)
                    return BadRequest(new { message = "Не удалось начать работу. Проверьте статус наряда." });

                return Ok(new { message = "Работа начата", taskId });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Завершить работу по наряду
        /// </summary>
        [HttpPost("my/{taskId}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteTask(int taskId, [FromBody] CompleteTaskRequest? request = null)
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                _logger.LogInformation("✅ Сотрудник #{TabNum} завершает наряд #{TaskId}", tabNumber, taskId);

                var success = await _taskService.CompleteTaskAsync(taskId, tabNumber);

                if (!success)
                    return BadRequest(new { message = "Не удалось завершить работу. Проверьте статус наряда." });

                return Ok(new { message = "Работа завершена", taskId, comment = request?.Comment });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отменить наряд
        /// </summary>
        [HttpPost("my/{taskId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelTask(int taskId, [FromBody] CancelTaskRequest? request = null)
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                var reason = request?.Reason ?? "Отменено инженером";

                _logger.LogInformation("❌ Сотрудник #{TabNum} отменяет наряд #{TaskId}, Причина: {Reason}",
                    tabNumber, taskId, reason);

                var success = await _taskService.CancelTaskAsync(taskId, tabNumber, reason);

                if (!success)
                    return BadRequest(new { message = "Не удалось отменить наряд" });

                return Ok(new { message = "Наряд отменён", taskId });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("my/{taskId}/diagnostic-act")]
        [ProducesResponseType(typeof(DiagnosticActResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DiagnosticActResponseDto>> CreateDiagnosticAct(
    int taskId,
    [FromBody] CreateDiagnosticActRequest request)
        {
            try
            {
                var engineerTabNum = GetCurrentUserTabNumber();

                _logger.LogInformation("📝 Инженер #{Engineer} создаёт акт для наряда #{TaskId}",
                    engineerTabNum, taskId);

                // Проверяем, что taskId из URL совпадает с телом запроса
                if (request.TaskId != taskId)
                {
                    _logger.LogWarning("⚠️ Несоответствие taskId: URL={UrlId}, Body={BodyId}", taskId, request.TaskId);
                    return BadRequest(new { message = "Несоответствие идентификатора наряда" });
                }

                // Вызываем сервис для создания акта
                var result = await _diagnosticActService.CreateActAsync(request, engineerTabNum);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ Не удалось создать акт: {Message}", result.Message);
                    return BadRequest(new { message = result.Message });
                }

                _logger.LogInformation("✅ Акт создан: ActCode={ActCode}", result.ActCode);

                return CreatedAtAction(nameof(GetTaskById), new { taskId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при создании акта диагностики");
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        // EngineerTaskController.cs или AdminRequestController.cs

        // EngineerTaskController.cs - CompleteRepairTask

        

        /// <summary>
        /// Получить акты по наряду
        /// </summary>
        [HttpGet("my/{taskId}/acts")]
        [ProducesResponseType(typeof(RepairActResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<RepairActResponse>> GetTaskActs(int taskId)
        {
            var tabNumber = GetCurrentUserTabNumber();

            // Проверяем, что наряд принадлежит инженеру
            var task = await _context.TaskEntities
                .FirstOrDefaultAsync(t => t.WorkOrderId == taskId && t.ServiceEngineerTabNumber == tabNumber);

            if (task == null)
                return NotFound(new { message = "Наряд не найден" });

            // Получаем акты
            var repairAct = await _context.RepairActs
                .FirstOrDefaultAsync(a => a.TaskId == taskId);

            var writeOffAct = await _context.SparePartsWriteOffEntities
                .FirstOrDefaultAsync(a => a.TaskEntityId == taskId);

            return Ok(new RepairActResponse
            {
                RepairActPath = repairAct?.FilePath,
                SpareWriteOffPath = writeOffAct?.FilePath,
                CreatedAt = repairAct?.CreatedAt ?? writeOffAct?.WriteOffDate
            });
        }

        // EngineerTaskController.cs

        /// <summary>
        /// Начать ВСЕ работы по заявке (все наряды)
        /// </summary>
        [HttpPost("request/{requestId}/start-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> StartAllTasksForRequest(int requestId)
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();

                // Получаем ВСЕ наряды для этой заявки
                var tasks = await _context.TaskEntities
                    .Where(t => t.RequestId == requestId &&
                               t.ServiceEngineerTabNumber == tabNumber)
                    .ToListAsync();

                if (!tasks.Any())
                    return NotFound(new { message = "Наряды не найдены" });

                // Начинаем все наряды
                foreach (var task in tasks)
                {
                    if (task.Status == "assigned")
                    {
                        task.Status = "in_progress";
                        task.StartedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Начато {tasks.Count} наряд(ов)",
                    tasksStarted = tasks.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка начала всех нарядов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("request/{requestId}/complete-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteAllTasksForRequest(int requestId)
        {
            try
            {
                var tabNumber = GetCurrentUserTabNumber();
                _logger.LogInformation("🔧 Завершение ВСЕХ нарядов для заявки #{RequestId} инженером #{TabNum}",
                    requestId, tabNumber);

                // Получаем ВСЕ наряды для этой заявки
                var tasks = await _context.TaskEntities
            .Include(t => t.RequestEntity)
                .ThenInclude(r => r.ClientEntity)
                    .ThenInclude(c => c.User)  // ✅ Загружаем пользователя клиента
            .Include(t => t.StafEntity)
                .ThenInclude(s => s.User)       // ✅ Загружаем пользователя инженера
            .Where(t => t.RequestId == requestId &&
                       t.ServiceEngineerTabNumber == tabNumber)
            .ToListAsync();



                if (!tasks.Any())
                    return NotFound(new { message = "Наряды не найдены" });

                // Получаем акт диагностики (берём из первого наряда)
                var diagnosticAct = await _context.DiagnosticActs
                    .Include(a => a.Works)
                        .ThenInclude(w => w.Work)
                    .Include(a => a.SpareParts)
                        .ThenInclude(s => s.Spare)
                    .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

                if (diagnosticAct == null)
                    return NotFound(new { message = "Акт диагностики не найден" });

                // 🔥 1. Завершаем все наряды
                foreach (var task in tasks)
                {
                    if (task.Status == "in_progress")
                    {
                        task.Status = "completed";
                        task.CompletedAt = DateTime.UtcNow;
                    }
                }

                // 🔥 2. Обновляем статус заявки
                var request = await _context.Requests.FindAsync(requestId);
                if (request != null)
                {
                    request.Status = RequestStatus.Completed;
                }

                // 🔥 3. Формируем ОБЩИЙ акт выполненных работ для ВСЕХ работ
                var worksList = diagnosticAct.Works
                    .Where(w => w.IsApprovedByClient == true)
                    .Select(w => w.Work?.WorkName ?? $"Работа #{w.WorkCode}")
                    .ToList();

                var workName = string.Join(", ", worksList);

                var pdfData = new RepairActData
                {
                    TaskId = tasks.First().WorkOrderId,
                    RequestId = requestId,
                    ClientFio = request?.ClientEntity?.User?.FullName ?? "Неизвестно",
                    DeviceModel = request?.Model ?? "",
                    SerialNumber = request?.SerialNumber ?? "",
                    WorkName = workName,
                    CompletedAt = DateTime.UtcNow,
                    EngineerFio = tasks.First().StafEntity?.User?.FullName ?? "Неизвестно",
                    EngineerTabNumber = tabNumber,
                    ClientSignedAt = null,
                    ClientSignatureMethod = null,
                    ClientIp = null,
                    DispatcherSignedAt = null,
                    DispatcherFio = null,
                    DispatcherTabNumber = null
                };

                var actPath = await _pdfService.GenerateRepairActAsync(pdfData);

                // 🔥 4. Сохраняем акт выполненных работ ДЛЯ КАЖДОГО наряда
                foreach (var task in tasks)
                {
                    var repairAct = new RepairActEntity
                    {
                        TaskId = task.WorkOrderId,
                        FilePath = actPath,
                        CreatedAt = DateTime.UtcNow,
                        Comment = $"Акт сформирован при завершении наряда #{task.WorkOrderId}",
                        IsSignedByClient = false,
                        IsSignedByDispatcher = false,
                        IsVerifiedByDispatcher = false,
                        SignedAt = null,
                        SignatureMethod = null,
                        ClientVerificationData = null,
                        DispatcherSignedAt = null,
                        DispatcherVerificationData = null,
                        DispatcherSignedByTabNumber = null
                    };
                    _context.RepairActs.Add(repairAct);

                    _logger.LogInformation("✅ Создан акт выполненных работ для наряда #{TaskId}", task.WorkOrderId);
                }

                // 🔥 5. Создаём акт списания ЗИП ДЛЯ КАЖДОГО наряда
                // Сначала получаем ОДИН акт диагностики для ВСЕЙ заявки (не для каждого наряда!)
                var requestDiagnosticAct = await _context.DiagnosticActs
                    .Include(a => a.SpareParts)
                        .ThenInclude(s => s.Spare)
                    .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);  // ← Ищем по requestId!

                if (requestDiagnosticAct?.SpareParts?.Any(s => s.IsApprovedByClient == true) == true)
                {
                    var approvedSpares = requestDiagnosticAct.SpareParts
                        .Where(s => s.IsApprovedByClient == true)
                        .ToList();

                    // Создаём записи в БД для каждого ЗИП для КАЖДОГО наряда
                    foreach (var task in tasks)
                    {
                        foreach (var spare in approvedSpares)
                        {
                            var supplierSpare = await _context.SupplierSpares
                                .FirstOrDefaultAsync(ss => ss.SpareCode == spare.SpareCode);

                            if (supplierSpare != null)
                            {
                                var writeOffEntry = new SparePartsWriteOffEntity
                                {
                                    TaskEntityId = task.WorkOrderId,  // ← Код текущего РЕМОНТНОГО наряда (#20, #21)
                                    SupplierSpareId = supplierSpare.Id,
                                    Quantity = spare.Amount,
                                    WriteOffDate = DateTime.UtcNow,
                                    Comment = $"Списание для наряда #{task.WorkOrderId}",
                                    WriteOffType = 0,
                                    FilePath = ""
                                };
                                _context.SparePartsWriteOffEntities.Add(writeOffEntry);
                            }
                        }

                        _logger.LogInformation("✅ Созданы записи списания ЗИП для наряда #{TaskId}: {Count} позиций",
                            task.WorkOrderId, approvedSpares.Count);
                    }

                    // Сохраняем записи, чтобы получить их коды
                    await _context.SaveChangesAsync();

                    // 🔥 6. Генерируем ОДИН PDF файл акта списания
                    var writeOffData = new SpareWriteOffData
                    {
                        TaskId = tasks.First().WorkOrderId,
                        RequestId = requestId,
                        ClientFio = request?.ClientEntity?.User?.FullName ?? "Неизвестно",
                        DeviceModel = request?.Model ?? "",
                        SerialNumber = request?.SerialNumber ?? "",
                        CompletedAt = DateTime.UtcNow,
                        Spares = approvedSpares.Select(s =>
                        {
                            // 🔥 Получаем цену из таблицы "ЗИП у поставщиков"
                            var supplierSpare = _context.SupplierSpares
                                .FirstOrDefault(ss => ss.SpareCode == s.SpareCode);

                            return new SpareItemData
                            {
                                SpareName = s.Spare?.SpareName ?? $"ЗИП #{s.SpareCode}",
                                Quantity = s.Amount,
                                UnitPrice = supplierSpare?.UnitPrice ?? 0,  // ← Реальная цена!
                            };
                        }).ToList()
                    };

                    var writeOffPath = await _pdfService.GenerateSpareWriteOffAsync(writeOffData);

                    // 🔥 7. Обновляем FilePath для ВСЕХ записей списания этих нарядов
                    var taskIds = tasks.Select(t => t.WorkOrderId).ToList();
                    var writeOffEntries = await _context.SparePartsWriteOffEntities
                        .Where(w => taskIds.Contains(w.TaskEntityId))
                        .ToListAsync();

                    foreach (var entry in writeOffEntries)
                    {
                        entry.FilePath = writeOffPath;
                    }

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("✅ Сгенерирован акт списания ЗИП: {Count} записей, файл: {Path}",
                        writeOffEntries.Count, writeOffPath);
                }
                else
                {
                    _logger.LogWarning("⚠️ Нет согласованных ЗИП в акте диагностики для заявки #{RequestId}", requestId);
                }

                return Ok(new
                {
                    message = $"Все работы по заявке #{requestId} завершены",
                    tasksCompleted = tasks.Count,
                    actsGenerated = true,
                    repairActPath = actPath,
                    repairActsCreated = tasks.Count,
                    writeOffRecordsCreated = tasks.Sum(t =>
                        diagnosticAct.SpareParts.Count(s => s.IsApprovedByClient == true))
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка завершения всех нарядов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }


    }

    public class RepairActResponse
    {
        public string? RepairActPath { get; set; }
        public string? SpareWriteOffPath { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    // DTO для отмены наряда
    public class CancelTaskRequest
    {
        public string? Reason { get; set; }
    }

}
