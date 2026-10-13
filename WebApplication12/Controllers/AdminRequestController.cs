using Diploma.Application.Services;
using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Diploma.Core.Models;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using static Diploma.Core.Class1;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Policy = "Authenticated")]
    [Produces("application/json")]
    public class AdminRequestController : ControllerBase
    {
        private readonly IRequestAdminService _service;
        private readonly ILogger<AdminRequestController> _logger;
        private readonly DiplomaDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IPdfGenerationService _pdfService;  // 🔥 Добавь это

        public AdminRequestController(IRequestAdminService service, ILogger<AdminRequestController> logger, DiplomaDbContext context, IWebHostEnvironment env, IPdfGenerationService pdfService)
        {
            _service = service;
            _context = context;
            _logger = logger;
            _env = env;
            _pdfService = pdfService;
        }
        [HttpGet]
        //[Authorize(Policy = "DispatcherOnly")]
        [ProducesResponseType(typeof(List<RequestAdmPanel>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<RequestAdmPanel>>> GetAllRequests(CancellationToken cancellationToken)
        {
            var requests = await _service.GetAllRequestsAsync();
            return Ok(requests);
        }

        // Diploma.API/Controllers/Dispatcher/RequestAdminController.cs

        //[HttpGet("request/{requestId}/spare-status")]
        //[ProducesResponseType(typeof(SpareStatusResult), StatusCodes.Status200OK)]
        //public async Task<ActionResult<SpareStatusResult>> GetSpareStatus(int requestId)
        //{
        //    _logger.LogInformation("🔍 Запрос статуса ЗИП для заявки #{RequestId}", requestId);

        //    try
        //    {
        //        // 1. Находим акт по заявке
        //        _logger.LogDebug("📋 Поиск акта для заявки #{RequestId}", requestId);

        //        var act = await _context.DiagnosticActs
        //            .Include(a => a.SpareParts)
        //            .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

        //        if (act == null)
        //        {
        //            _logger.LogWarning("⚠️ Акт для заявки #{RequestId} не найден", requestId);
        //            return NotFound(new { message = "Акт не найден" });
        //        }

        //        _logger.LogInformation("✅ Акт найден: ActCode={ActCode}, TaskId={TaskId}",
        //            act.ActCode, act.TaskId);

        //        // 2. Логируем ЗИП в акте
        //        var actSpares = act.SpareParts?.ToList() ?? new List<SparePartsDiagnosticAct>();
        //        _logger.LogInformation("📦 ЗИП в акте #{ActCode}: {Count} позиций",
        //            act.ActCode, actSpares.Count);

        //        foreach (var spare in actSpares)
        //        {
        //            _logger.LogDebug("  - ЗИП #{SpareCode}, кол-во: {Amount}, согласовано: {Approved}",
        //                spare.SpareCode, spare.Amount, spare.IsApprovedByClient);
        //        }

        //        // 3. Проверяем заявки на ЗИП
        //        _logger.LogDebug("🔎 Поиск неподтверждённых заявок для акта #{ActCode}", act.ActCode);

        //        var allOrdersForAct = await _context.SpareOrders
        //            .Include(o => o.SupplierSpare)
        //                .ThenInclude(ss => ss.Spare)
        //            .Include(o => o.SupplierSpare)
        //                .ThenInclude(ss => ss.Supplier)
        //            .Where(o => o.ActCode == act.ActCode)
        //            .ToListAsync();

        //        _logger.LogInformation("📊 Всего заявок для акта #{ActCode}: {Count}",
        //            act.ActCode, allOrdersForAct.Count);

        //        foreach (var order in allOrdersForAct)
        //        {
        //            _logger.LogDebug("  - OrderId={OrderId}, SupplierSpareId={SupplierSpareId}, " +
        //                           "Quantity={Qty}, Status={Status} ({StatusInt}), SpareName={SpareName}",
        //                order.OrderId, order.SupplierSpareId, order.Quantity,
        //                order.Status, (int)order.Status,
        //                order.SupplierSpare?.Spare?.SpareName ?? "N/A");
        //        }

        //        // 4. Фильтруем только неподтверждённые (Reservation = 0)
        //        var pendingOrders = allOrdersForAct
        //            .Where(o => o.Status == DataAccess.Entities.SpareOrderStatus.Reservation)
        //            .ToList();

        //        _logger.LogInformation("⏳ Неподтверждённых заявок (статус 0): {Count}",
        //            pendingOrders.Count);

        //        // 5. Формируем результат
        //        var result = new SpareStatusResult
        //        {
        //            ActCode = act.ActCode,
        //            HasPendingOrders = pendingOrders.Any(),
        //            PendingOrdersCount = pendingOrders.Count,
        //            PendingOrders = pendingOrders.Select(o => new PendingOrderInfo
        //            {
        //                SpareName = o.SupplierSpare?.Spare?.SpareName ?? $"ЗИП #{o.SupplierSpareId}",
        //                Quantity = o.Quantity,
        //                SupplierName = o.SupplierSpare?.Supplier?.Name ?? $"Поставщик #{o.SupplierSpare?.SupplierId}"
        //            }).ToList()
        //        };

        //        _logger.LogInformation("📤 Возвращаем результат: HasPendingOrders={HasPending}, Count={Count}",
        //            result.HasPendingOrders, result.PendingOrdersCount);

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "❌ Ошибка при получении статуса ЗИП для заявки #{RequestId}", requestId);
        //        return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
        //    }
        //}
        public class SpareStockStatusDto
        {
            public int SpareCode { get; set; }
            public string SpareName { get; set; } = string.Empty;
            public int RequiredAmount { get; set; }      // Сколько нужно по акту
            public int InStock { get; set; }             // Сколько есть на складе
            public int Shortage => Math.Max(0, RequiredAmount - InStock); // Дефицит
            public bool IsSufficient => InStock >= RequiredAmount;        // Хватает ли на складе
        }

        [HttpGet("request/{requestId}/spare-status")]
        [ProducesResponseType(typeof(List<SpareStockStatusDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<SpareStockStatusDto>>> GetSpareStatus(int requestId)
        {
            _logger.LogInformation("🔍 Запрос потребности в ЗИП для заявки #{RequestId}", requestId);

            try
            {
                // 1. Находим акт диагностики по заявке
                var act = await _context.DiagnosticActs
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.Task.RequestId == requestId);

                if (act == null)
                {
                    _logger.LogWarning("⚠️ Акт для заявки #{RequestId} не найден", requestId);
                    return NotFound(new { message = "Акт не найден" });
                }

                _logger.LogInformation("✅ Акт найден: ActCode={ActCode}", act.ActCode);

                // 2. Получаем ЗИП из акта (потребность) и текущие остатки на складе одним запросом
                var spareStatuses = await _context.SparePartsDiagnosticActs
                    .Include(sp => sp.Spare)
                    .Where(sp => sp.ActCode == act.ActCode)
                    .Select(sp => new
                    {
                        SpareCode = sp.SpareCode,
                        SpareName = sp.Spare != null ? sp.Spare.SpareName : "Неизвестно",
                        RequiredAmount = sp.Amount
                    })
                    .ToListAsync();

                if (!spareStatuses.Any())
                {
                    _logger.LogInformation("📭 В акте #{ActCode} нет ЗИП", act.ActCode);
                    return Ok(new List<SpareStockStatusDto>());
                }

                // 3. Получаем коды ЗИП для загрузки остатков
                var spareCodes = spareStatuses.Select(s => s.SpareCode).ToList();

                // 4. Загружаем текущие остатки из "Учёт ЗИП"
                var stockLevels = await _context.Spares
                    .Where(s => spareCodes.Contains(s.SpareCode))
                    .Select(s => new { s.SpareCode, s.Amount })
                    .ToDictionaryAsync(s => s.SpareCode, s => s.Amount);

                var spareOrders = await _context.SpareOrders.
                    Where(s => s.ActCode == act.ActCode)
                    .Select(s => new { s.OrderId, s.SupplierSpareId, s.Quantity })
                    .ToListAsync();

                
                if(spareOrders.Count > 0)
                {
                    var res = spareStatuses.Select(sp => new SpareStockStatusDto
                    {
                        SpareCode = sp.SpareCode,
                        SpareName = sp.SpareName,
                        RequiredAmount = 0,
                        InStock = stockLevels.TryGetValue(sp.SpareCode, out var stock) ? stock : 0
                    }).ToList();
                    _logger.LogInformation("Заявка уже была создана");
                    return Ok(res);
                }
                
                // 5. Формируем результат
                var result = spareStatuses.Select(sp => new SpareStockStatusDto
                {
                    SpareCode = sp.SpareCode,
                    SpareName = sp.SpareName,
                    RequiredAmount = sp.RequiredAmount,
                    InStock = stockLevels.TryGetValue(sp.SpareCode, out var stock) ? stock : 0
                }).ToList();

                _logger.LogInformation("📦 Обработано {Count} позиций ЗИП. Дефицитных: {ShortageCount}",
                    result.Count, result.Count(r => r.Shortage > 0));

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении потребности в ЗИП для заявки #{RequestId}", requestId);
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        [HttpPost("request/{requestId}/confirm-spare-orders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmSpareOrders(int requestId)
        {
            _logger.LogInformation("📦 Запрос подтверждения заявок на ЗИП для заявки #{RequestId}", requestId);

            try
            {
                var act = await _context.DiagnosticActs
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

                if (act == null)
                {
                    _logger.LogWarning("⚠️ Акт для заявки #{RequestId} не найден", requestId);
                    return NotFound(new { message = "Акт не найден" });
                }

                _logger.LogDebug("🔎 Поиск неподтверждённых заявок для акта #{ActCode}", act.ActCode);

                var pendingOrders = await _context.SpareOrders
                    .Where(o => o.ActCode == act.ActCode &&
                                o.Status == DataAccess.Entities.SpareOrderStatus.Reservation)
                    .ToListAsync();

                _logger.LogInformation("📊 Найдено неподтверждённых заявок: {Count}", pendingOrders.Count);

                foreach (var order in pendingOrders)
                {
                    _logger.LogDebug("  - OrderId={OrderId}, SupplierSpareId={SupplierSpareId}, Qty={Qty}",
                        order.OrderId, order.SupplierSpareId, order.Quantity);
                }

                if (!pendingOrders.Any())
                {
                    _logger.LogWarning("⚠️ Нет заявок для подтверждения (акт #{ActCode})", act.ActCode);
                    return BadRequest(new { message = "Нет заявок для подтверждения" });
                }

                // Меняем статус на Confirmed (1)
                foreach (var order in pendingOrders)
                {
                    _logger.LogDebug("🔄 Обновление статуса заявки #{OrderId}: Reservation → Confirmed",
                        order.OrderId);
                    order.Status = DataAccess.Entities.SpareOrderStatus.Confirmed;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Подтверждено {Count} заявок на ЗИП для акта #{ActCode}",
                    pendingOrders.Count, act.ActCode);

                return Ok(new
                {
                    message = "Заявки на ЗИП отправлены в ОМТС",
                    ordersConfirmed = pendingOrders.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при подтверждении заявок для заявки #{RequestId}", requestId);
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        /// <summary>
        /// Получить акты по завершённой заявке
        /// </summary>
        [HttpGet("{requestId}/acts")]
        [ProducesResponseType(typeof(TaskActsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaskActsResponse>> GetRequestActs(int requestId)
        {
            try
            {
                _logger.LogInformation("🔍 Запрос актов для заявки #{RequestId}", requestId);

                // 1. Проверяем, что заявка существует и завершена
                var request = await _context.Requests
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                    return NotFound(new { message = "Заявка не найдена" });

                if (request.Status != RequestStatus.Completed)
                    return BadRequest(new
                    {
                        message = "Акты доступны только для завершённых заявок",
                        currentStatus = request.Status
                    });

                // 2. Находим наряд по заявке
                var task = await _context.TaskEntities
                    .FirstOrDefaultAsync(t => t.RequestId == requestId);

                if (task == null)
                    return NotFound(new { message = "Наряд не найден" });

                // 3. Получаем акты
                var repairAct = await _context.RepairActs
                    .FirstOrDefaultAsync(a => a.TaskId == task.WorkOrderId);

                var writeOffAct = await _context.SparePartsWriteOffEntities
                    .FirstOrDefaultAsync(a => a.TaskEntityId == task.WorkOrderId);

                _logger.LogInformation("✅ Акты найдены: Repair={RepairPath}, WriteOff={WriteOffPath}",
                    repairAct?.FilePath, writeOffAct?.FilePath);


                // 🔥 ДОБАВЬТЕ actId в ответ!
                return Ok(new TaskActsResponse
                {
                    ActId = repairAct?.Id,  // ← НОВОЕ ПОЛЕ!
                    RepairActPath = repairAct?.FilePath,
                    SpareWriteOffPath = writeOffAct?.FilePath,
                    CreatedAt = repairAct?.CreatedAt ?? writeOffAct?.WriteOffDate
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка получения актов для заявки #{RequestId}", requestId);
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        // Diploma.API/Controllers/AdminRequestController.cs

        /// <summary>
        /// Отметить акт как подписанный (клиент в офисе) - для ВСЕХ нарядов заявки
        /// </summary>
        [HttpPost("{requestId}/acts/sign-office")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SignActOffice(int requestId)
        {
            try
            {
                _logger.LogInformation("📝 Подписание акта в офисе для заявки #{RequestId}", requestId);

                // 1. Находим ВСЕ наряды по заявке
                var tasks = await _context.TaskEntities
                    .Where(t => t.RequestId == requestId)
                    .ToListAsync();

                if (!tasks.Any())
                    return NotFound(new { message = "Наряды не найдены" });

                // 2. Находим ВСЕ акты по этим нарядам
                var taskIds = tasks.Select(t => t.WorkOrderId).ToList();
                var acts = await _context.RepairActs
                    .Where(a => taskIds.Contains(a.TaskId ?? 0))
                    .ToListAsync();

                if (!acts.Any())
                    return NotFound(new { message = "Акты не найдены" });

                // 3. Отмечаем подписание для ВСЕХ актов
                foreach (var act in acts)
                {
                    act.IsSignedByClient = true;
                    act.SignedAt = DateTime.UtcNow;
                    act.SignatureMethod = "office";
                    _logger.LogInformation("✅ Акт подписан: ActId={ActId}", act.Id);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Акт подписан для наряд(ов)",
                    signedAt = DateTime.UtcNow,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка подписания акта");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Отметить оплату как полученную
        /// </summary>
        [HttpPost("{requestId}/payment/received")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> MarkPaymentReceived(int requestId)
        {
            try
            {
                _logger.LogInformation("💰 Отметка оплаты для заявки #{RequestId}", requestId);

                // 1. Проверяем, что акт подписан
                var task = await _context.TaskEntities
                    .Include(t => t.RequestEntity)
                    .FirstOrDefaultAsync(t => t.RequestId == requestId);

                if (task?.RequestEntity == null)
                    return NotFound(new { message = "Заявка не найдена" });

                var act = await _context.RepairActs
                    .FirstOrDefaultAsync(a => a.TaskId == task.WorkOrderId);

                if (act == null || !act.IsSignedByClient)
                    return BadRequest(new
                    {
                        message = "Сначала отметьте подписание акта",
                        actSigned = act?.IsSignedByClient ?? false
                    });

                // 2. Отмечаем оплату
                act.IsVerifiedByDispatcher = true;
                task.RequestEntity.Status = RequestStatus.Completed; // или отдельный статус "Оплачено"

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Оплата отмечена: RequestId={RequestId}", requestId);

                return Ok(new
                {
                    message = "Оплата подтверждена",
                    paidAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка отметки оплаты");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Сформировать гарантийный талон
        /// </summary>
        [HttpPost("{requestId}/warranty/generate")]
        [ProducesResponseType(typeof(WarrantyResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateWarranty(int requestId)
        {
            try
            {
                _logger.LogInformation("📄 Генерация гарантийного талона для заявки #{RequestId}", requestId);

                // 1. Проверяем заявку и акт
                // 1. Проверяем заявку и акт
                var request = await _context.Requests
                    .Include(r => r.ClientEntity)
                        .ThenInclude(c => c.User)  // 🔥 ДОБАВЬТЕ ЭТУ СТРОКУ!
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                    return NotFound(new { message = "Заявка не найдена" });

                var task = await _context.TaskEntities
                    .FirstOrDefaultAsync(t => t.RequestId == requestId);

                if (task == null)
                    return NotFound(new { message = "Наряд не найден" });

                var act = await _context.RepairActs
    .Include(a => a.Signature)  // 🔥 ДОБАВЬТЕ ЭТУ СТРОКУ!
    .FirstOrDefaultAsync(a => a.TaskId == task.WorkOrderId);

                if (act == null)
                    return NotFound(new { message = "Акт выполненных работ не найден" });

                // 🔥 ПРОВЕРКА ПОДПИСИ: акт должен быть подписан клиентом (офис или электронно)
                if (!act.IsSignedByClient)
                    return BadRequest(new
                    {
                        message = "Акт должен быть подписан клиентом",
                        isSigned = act.IsSignedByClient
                    });

                // 🔥 ПРОВЕРКА ОПЛАТЫ: либо через акт (офис), либо через финальный счёт (электронно)
                var isPaidViaAct = act.IsVerifiedByDispatcher;  // Оплата в офисе
                var isPaidViaInvoice = await _context.Invoices
                    .AnyAsync(i => i.RequestId == requestId &&
                                  i.InvoiceType == "final" &&
                                  i.Status == InvoiceStatus.Paid);  // Оплата электронно

                if (!isPaidViaAct && !isPaidViaInvoice)
                    return BadRequest(new
                    {
                        message = "Оплата должна быть подтверждена (в офисе или через финальный счёт)",
                        isPaidViaAct = isPaidViaAct,
                        isPaidViaInvoice = isPaidViaInvoice
                    });

                // 2. Проверяем, нет ли уже выданного талона
                var existingWarranty = await _context.WarrantyCards
                    .FirstOrDefaultAsync(w => w.RequestId == requestId && w.IsActive);

                if (existingWarranty != null)
                    return Ok(new WarrantyResponse
                    {
                        Message = "Талон уже сформирован",
                        WarrantyPath = existingWarranty.FilePath,
                        ValidUntil = existingWarranty.ValidUntil
                    });

                // 3. Генерируем PDF
                var warrantyData = new WarrantyData
                {
                    RequestId = requestId,
                    ActId = act.Id,
                    ClientFio = request.ClientEntity?.User.FullName ?? "Неизвестно",
                    DeviceModel = request.Model ?? "",
                    SerialNumber = request.SerialNumber ?? "",
                    RepairDate = act.CreatedAt,
                    WarrantyMonths = 3
                };

                var warrantyPath = await _pdfService.GenerateWarrantyCardAsync(warrantyData);

                // 4. Сохраняем запись о талоне
                var warrantyCard = new WarrantyCardEntity
                {
                    RequestId = requestId,
                    ActId = act.Id,
                    FilePath = warrantyPath,
                    IssuedAt = DateTime.UtcNow,
                    WarrantyMonths = 3,
                    ValidUntil = DateTime.UtcNow.AddMonths(3),
                    IsActive = true
                };

                _context.WarrantyCards.Add(warrantyCard);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Гарантийный талон сформирован: WarrantyId={WarrantyId}", warrantyCard.Id);

                return Ok(new WarrantyResponse
                {
                    Message = "Гарантийный талон сформирован",
                    WarrantyPath = warrantyPath,
                    ValidUntil = warrantyCard.ValidUntil
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка генерации гарантийного талона");
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        /// <summary>
        /// Получить статус финализации заявки (акт, оплата, гарантия)
        /// </summary>
        [HttpGet("{requestId}/finalization-status")]
        [ProducesResponseType(typeof(FinalizationStatusResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<FinalizationStatusResponse>> GetFinalizationStatus(int requestId)
        {
            var task = await _context.TaskEntities
                .FirstOrDefaultAsync(t => t.RequestId == requestId);

            if (task == null)
                return NotFound(new { message = "Наряд не найден" });

            var act = await _context.RepairActs
                .FirstOrDefaultAsync(a => a.TaskId == task.WorkOrderId);

            var warranty = await _context.WarrantyCards
                .FirstOrDefaultAsync(w => w.RequestId == requestId && w.IsActive);

            return Ok(new FinalizationStatusResponse
            {
                ActExists = act != null,
                ActSigned = act?.IsSignedByClient ?? false,
                ActSignedAt = act?.SignedAt,
                PaymentReceived = act?.IsVerifiedByDispatcher ?? false,
                WarrantyExists = warranty != null,
                WarrantyPath = warranty?.FilePath,
                WarrantyValidUntil = warranty?.ValidUntil,
                // 🔥 Подписи
                ActSignedByClient = act?.IsSignedByClient ?? false,
                ClientSignedAt = act?.SignedAt,
                ClientSignatureMethod = act?.SignatureMethod,
                ActSignedByDispatcher = act?.IsSignedByDispatcher ?? false,
                DispatcherSignedAt = act?.DispatcherSignedAt,
                DispatcherSignedByTabNumber = act?.DispatcherSignedByTabNumber,
                IsVerifiedByDispatcher = act?.IsVerifiedByDispatcher ?? false
            });
        }

        /// <summary>
        /// Отправить гарантийный талон клиенту (сделать доступным в ЛК)
        /// </summary>
        [HttpPost("{requestId}/warranty/send-to-client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendWarrantyToClient(int requestId)
        {
            try
            {
                _logger.LogInformation("📤 Отправка гарантийного талона клиенту для заявки #{RequestId}", requestId);

                // 1. Проверяем заявку
                var request = await _context.Requests
                    .Include(r => r.ClientEntity)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                    return NotFound(new { message = "Заявка не найдена" });

                // 2. Проверяем гарантийный талон
                var warranty = await _context.WarrantyCards
                    .FirstOrDefaultAsync(w => w.RequestId == requestId && w.IsActive);

                if (warranty == null)
                    return NotFound(new { message = "Гарантийный талон не найден" });

                // 3. Отмечаем как отправленный
                warranty.IsVisibleToClient = true;

                // 4. 🔥 Опционально: Уведомление клиенту (email/SMS)
                // await _notificationService.SendWarrantyNotificationAsync(
                //     request.ClientEntityId, 
                //     warranty.FilePath,
                //     warranty.ValidUntil
                // );

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Гарантийный талон отправлен клиенту: WarrantyId={WarrantyId}", warranty.Id);

                return Ok(new
                {
                    message = "Гарантийный талон отправлен клиенту",
                    warrantyPath = warranty.FilePath,
                    validUntil = warranty.ValidUntil
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка отправки гарантийного талона");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("{requestId}/acts/mark-client-signed")]
        public async Task<IActionResult> MarkClientSigned(int requestId, [FromBody] MarkSignedRequest? request = null)
        {
            try
            {
                var dispatcherTab = GetCurrentDispatcherTabNumber();
                var signatureMethod = request?.SignatureMethod ?? "office_handwritten";

                // 1-2. Загрузка нарядов и актов (аналогично выше)
                var tasks = await _context.TaskEntities.Where(t => t.RequestId == requestId).ToListAsync();
                if (!tasks.Any()) return NotFound(new { message = "Наряды не найдены" });

                var taskIds = tasks.Select(t => t.WorkOrderId).ToList();
                var acts = await _context.RepairActs
                    .Include(a => a.Signature)  // 🔥 Загружаем Signature
                    .Where(a => taskIds.Contains(a.TaskId ?? 0))
                    .ToListAsync();

                if (!acts.Any()) return NotFound(new { message = "Акты не найдены" });

                // 3. Обновляем клиентскую подпись в СУЩЕСТВУЮЩЕЙ записи
                foreach (var act in acts)
                {
                    if (act.Signature != null)
                    {
                        act.Signature.IsSignedByClient = true;
                        act.Signature.SignedAt = DateTime.UtcNow;
                        act.Signature.SignatureMethod = signatureMethod;
                        act.Signature.ClientVerificationData = JsonSerializer.Serialize(new
                        {
                            marked_by_dispatcher = dispatcherTab,
                            marked_at = DateTime.UtcNow,
                            method = signatureMethod
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Подписание клиента отмечено" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка отметки подписи клиента");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("{requestId}/acts/dispatcher-sign")]
        public async Task<IActionResult> DispatcherSignAct(int requestId)
        {
            try
            {
                var dispatcherTab = GetCurrentDispatcherTabNumber();
                var dispatcherName = User.Identity?.Name;

                _logger.LogInformation("✍️ Диспетчер #{TabNum} подписывает акт для заявки #{RequestId}",
                    dispatcherTab, requestId);

                // 1. Находим ВСЕ наряды по заявке
                var tasks = await _context.TaskEntities
                    .Where(t => t.RequestId == requestId)
                    .ToListAsync();

                if (!tasks.Any())
                    return NotFound(new { message = "Наряды не найдены" });

                // 2. Находим ВСЕ акты с загрузкой Signature (🔥 КЛЮЧЕВОЕ ИЗМЕНЕНИЕ)
                var taskIds = tasks.Select(t => t.WorkOrderId).ToList();
                var acts = await _context.RepairActs
                    .Include(a => a.Signature)  // 🔥 Загружаем связанную подпись
                    .Where(a => taskIds.Contains(a.TaskId ?? 0))
                    .ToListAsync();

                if (!acts.Any())
                    return NotFound(new { message = "Акты не найдены" });

                // 3. Обновляем данные в СУЩЕСТВУЮЩИХ записях Signature
                foreach (var act in acts)
                {
                    // 🔥 Обновляем только если Signature существует
                    if (act.Signature != null)
                    {
                        act.Signature.IsSignedByDispatcher = true;
                        act.Signature.DispatcherSignedAt = DateTime.UtcNow;
                        act.Signature.DispatcherSignedByTabNumber = dispatcherTab;
                        act.Signature.DispatcherVerificationData = JsonSerializer.Serialize(new
                        {
                            dispatcher_tab = dispatcherTab,
                            dispatcher_name = dispatcherName,
                            ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            user_agent = Request.Headers["User-Agent"].ToString(),
                            signed_at = DateTime.UtcNow
                        });

                        _logger.LogInformation("✅ Подпись обновлена: ActId={ActId}, SignatureId={SigId}",
                            act.Id, act.SignatureId);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ У акта #{ActId} нет связанной записи подписи", act.Id);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Акты подписаны диспетчером",
                    signedAt = DateTime.UtcNow,
                    signedCount = acts.Count(a => a.Signature?.IsSignedByDispatcher == true)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка подписания акта диспетчером");
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        /// <summary>
        /// Получить табельный номер текущего диспетчера из JWT-токена
        /// </summary>
        private int GetCurrentDispatcherTabNumber()
        {
            // 🔍 Пробуем несколько вариантов claim для табельного номера
            var tabClaim = User.FindFirst("tabNumber")
                        ?? User.FindFirst("TabNum")
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub");

            if (tabClaim != null && int.TryParse(tabClaim.Value, out var tabNumber))
            {
                _logger.LogDebug("✅ Текущий диспетчер: TabNum={TabNum}, Login={Login}",
                    tabNumber, User.Identity?.Name);
                return tabNumber;
            }

            // 🔥 Если не нашли табельный номер, возвращаем 0 или выбрасываем исключение
            _logger.LogWarning("⚠️ Не удалось найти табельный номер диспетчера в токене. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));

            // Вариант 1: Вернуть 0 (для тестирования)
            return 0;

            // Вариант 2: Выбросить исключение (для продакшена)
            // throw new UnauthorizedAccessException("Не удалось определить диспетчера");
        }

        // AdminRequestController.cs

        // ✅ Вместо этого просто верните OK:
        [HttpPost("{requestId}/acts/send-to-client")]
        public async Task<IActionResult> SendActsToClient(int requestId)
        {
            // Просто подтверждаем, что акты готовы к отправке
            // Фактическое отслеживание будем делать через статусы подписей

            var act = await _context.RepairActs
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

            if (act == null)
                return NotFound(new { message = "Акт не найден" });

            // Проверяем, что акты сгенерированы
            if (string.IsNullOrEmpty(act.FilePath))
                return BadRequest(new { message = "Акты ещё не сгенерированы" });

            return Ok(new
            {
                message = "Акты готовы к отправке клиенту",
                actId = act.Id,
                filePath = act.FilePath
            });
        }

        /// <summary>
        /// Получить статус подписания акта
        /// </summary>
        [HttpGet("{requestId}/acts/signature-status")]
        public async Task<ActionResult<ActSignatureStatus>> GetActSignatureStatus(int requestId)
        {
            _logger.LogInformation("🔍 Поиск статуса подписи для заявки #{RequestId}", requestId);

            // 1. Находим все TaskId для этой заявки
            var taskIds = await _context.TaskEntities
                .Where(t => t.RequestId == requestId)
                .Select(t => t.WorkOrderId)
                .ToListAsync();

            _logger.LogInformation("  Найдено нарядов: {Count}, IDs: {Ids}",
                taskIds.Count, string.Join(", ", taskIds));

            if (!taskIds.Any())
                return NotFound(new { message = "Наряды не найдены" });

            // 2. 🔥 Ищем ВСЕ акты для этих нарядов с загрузкой Signature
            var allActs = await _context.RepairActs
                .Include(a => a.Signature)  // 🔥 Загружаем подпись для каждого акта!
                .Where(a => a.TaskId.HasValue && taskIds.Contains(a.TaskId.Value))
                .ToListAsync();

            _logger.LogInformation("  Найдено актов для агрегации: {Count}", allActs.Count);
            foreach (var a in allActs)
            {
                _logger.LogDebug("    ActId={ActId}, TaskId={TaskId}, SignedByClient={Signed}, SignatureId={SigId}",
                    a.Id, a.TaskId, a.IsSignedByClient, a.SignatureId);
            }

            if (!allActs.Any())
            {
                _logger.LogWarning("⚠️ Акты не найдены для заявки #{RequestId}, taskIds: {TaskIds}",
                    requestId, string.Join(", ", taskIds));
                return NotFound(new { message = "Акты не найдены" });
            }

            // 3. 🔥 Агрегируем статусы ВСЕХ актов
            var allSignedByClient = allActs.All(a => a.IsSignedByClient);
            var allSignedByDispatcher = allActs.All(a => a.IsSignedByDispatcher);
            var signedByClientCount = allActs.Count(a => a.IsSignedByClient);
            var signedByDispatcherCount = allActs.Count(a => a.IsSignedByDispatcher);

            // Берём данные из первого акта для полей, которые не агрегируются
            var firstAct = allActs.First();

            _logger.LogInformation("  Статусы: AllSignedByClient={AllClient}, AllSignedByDispatcher={AllDisp}, SignedCount={Signed}/{Total}",
                allSignedByClient, allSignedByDispatcher, signedByClientCount, allActs.Count);

            // 4. Возвращаем агрегированный статус
            return Ok(new ActSignatureStatus
            {
                ActId = firstAct.Id,
                RequestId = requestId,

                // 🔥 Агрегированные статусы
                IsSignedByClient = allSignedByClient,
                ClientSignedAt = allActs.Max(a => a.SignedAt),
                ClientSignatureMethod = allActs.FirstOrDefault(a => a.IsSignedByClient)?.SignatureMethod,
                ClientVerificationData = allActs.FirstOrDefault(a => a.IsSignedByClient)?.ClientVerificationData,

                IsSignedByDispatcher = allSignedByDispatcher,
                DispatcherSignedAt = allActs.Max(a => a.DispatcherSignedAt),
                DispatcherSignedByTabNumber = allActs.FirstOrDefault(a => a.IsSignedByDispatcher)?.DispatcherSignedByTabNumber,
                DispatcherVerificationData = allActs.FirstOrDefault(a => a.IsSignedByDispatcher)?.DispatcherVerificationData,

                IsVerifiedByDispatcher = firstAct.IsVerifiedByDispatcher,

                IsFullySigned = allSignedByClient && allSignedByDispatcher,

                // 🔥 Счётчики
                ActsCount = allActs.Count,
                SignedActsCount = signedByClientCount
            });
        }

        // AdminRequestController.cs

        // AdminRequestController.cs - RegenerateActWithSignatures

        [HttpPost("{requestId}/acts/regenerate-with-signatures")]
        public async Task<IActionResult> RegenerateActWithSignatures(int requestId)
        {
            try
            {
                _logger.LogInformation("🔄 Перегенерация акта с подписями для заявки #{RequestId}", requestId);

                // 1. Находим ВСЕ наряды для этой заявки
                var tasks = await _context.TaskEntities
                    .Where(t => t.RequestId == requestId)
                    .ToListAsync();

                if (!tasks.Any())
                    return NotFound(new { message = "Наряды не найдены" });

                var taskIds = tasks.Select(t => t.WorkOrderId).ToList();

                // 2. 🔥 Находим ВСЕ акты для этих нарядов
                var allActs = await _context.RepairActs
                    .Include(a => a.Signature)  // 🔥 ДОБАВЬТЕ ЭТУ СТРОКУ!
                    .Include(a => a.Task)
                        .ThenInclude(t => t!.RequestEntity)
                            .ThenInclude(r => r.ClientEntity)
                                .ThenInclude(c => c.User)
                    .Include(a => a.Task)
                        .ThenInclude(t => t!.StafEntity)
                            .ThenInclude(s => s.User)
                    .Where(a => taskIds.Contains(a.TaskId ?? 0))
                    .ToListAsync();

                _logger.LogInformation("  Найдено актов: {Count}", allActs.Count);

                if (!allActs.Any())
                    return NotFound(new { message = "Акты не найдены" });

                // 3. 🔥 Проверяем, что ВСЕ акты подписаны обеими сторонами
                var allSignedByClient = allActs.All(a => a.IsSignedByClient);
                var allSignedByDispatcher = allActs.All(a => a.IsSignedByDispatcher);

                _logger.LogInformation("  Подписано клиентом: {Client}, Диспетчером: {Dispatcher}",
                    allSignedByClient, allSignedByDispatcher);

                if (!allSignedByClient || !allSignedByDispatcher)
                {
                    return BadRequest(new
                    {
                        message = "Все акты должны быть подписаны обеими сторонами",
                        totalActs = allActs.Count,
                        signedByClient = allActs.Count(a => a.IsSignedByClient),
                        signedByDispatcher = allActs.Count(a => a.IsSignedByDispatcher),
                        isSignedByClient = allSignedByClient,
                        isSignedByDispatcher = allSignedByDispatcher
                    });
                }

                // 4. 🔥 Берём первый акт для генерации (или можно объединить все)
                var firstAct = allActs.First();

                // 5. Получаем данные ДИСПЕТЧЕРА
                var dispatcher = await _context.Stafs
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.TabNum == firstAct.DispatcherSignedByTabNumber);

                // 6. Генерируем акт С подписями
                var pdfData = new RepairActData
                {
                    TaskId = firstAct.TaskId ?? 0,
                    RequestId = requestId,
                    ClientFio = firstAct.Task?.RequestEntity?.ClientEntity?.User?.FullName ?? "Неизвестно",
                    DeviceModel = firstAct.Task?.RequestEntity?.Model ?? "",
                    SerialNumber = firstAct.Task?.RequestEntity?.SerialNumber ?? "",
                    WorkName = "Ремонтные работы",
                    CompletedAt = firstAct.CreatedAt,
                    EngineerFio = firstAct.Task?.StafEntity?.User?.FullName ?? "Неизвестно",
                    EngineerTabNumber = firstAct.Task?.ServiceEngineerTabNumber ?? 0,

                    // Данные о подписях
                    ClientSignedAt = firstAct.SignedAt,
                    ClientSignatureMethod = firstAct.SignatureMethod,
                    ClientIp = firstAct.ClientVerificationData != null ?
                        JsonDocument.Parse(firstAct.ClientVerificationData).RootElement.GetProperty("ip").GetString() : null,

                    DispatcherSignedAt = firstAct.DispatcherSignedAt,
                    DispatcherFio = dispatcher?.User?.FullName ?? "Неизвестно",
                    DispatcherTabNumber = firstAct.DispatcherSignedByTabNumber
                };

                var actPath = await _pdfService.GenerateRepairActAsync(pdfData);

                // 7. 🔥 Обновляем путь для ВСЕХ актов
                foreach (var act in allActs)
                {
                    act.FilePath = actPath;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Акт перегенерирован с подписями: {Path}", actPath);

                return Ok(new { message = "Акт перегенерирован с подписями", actPath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка перегенерации акта");
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        /// <summary>
        /// Сформировать счёт на оплату работ для завершённой заявки
        /// (ЗИП уже оплачен на этапе предоплаты)
        /// </summary>

        /// <summary>
        /// Сформировать счёт на оплату работ для завершённой заявки
        /// (ЗИП уже оплачен на этапе предоплаты)
        /// </summary>
        /// <summary>
        /// Сформировать счёт на оплату работ для завершённой заявки
        /// (ЗИП уже оплачен на этапе предоплаты)
        /// </summary>
        [HttpPost("{requestId}/invoice/generate")]
        [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GenerateInvoice(int requestId)
        {
            try
            {
                _logger.LogInformation("🧾 Генерация счёта на работы для заявки #{RequestId}", requestId);

                // 1. Проверяем заявку
                // 1. Проверяем заявку
                var request = await _context.Requests
                    .Include(r => r.ClientEntity)
                        .ThenInclude(c => c.User)  // 🔥 ДОБАВЬТЕ ЭТУ СТРОКУ!
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request == null)
                    return NotFound(new { message = "Заявка не найдена" });

                // 2. Находим наряд
                var task = await _context.TaskEntities
                    .FirstOrDefaultAsync(t => t.RequestId == requestId);

                if (task == null)
                    return NotFound(new { message = "Наряд не найден" });

                // 3. 🔥 ИСПРАВЛЕНО: Ищем акт диагностики ПО ЗАЯВКЕ, а не по наряду!
                var diagnosticAct = await _context.DiagnosticActs
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

                if (diagnosticAct == null)
                    return NotFound(new
                    {
                        message = "Акт диагностики не найден. Убедитесь, что диагностика завершена и работы согласованы.",
                        requestId = requestId
                    });

                // 4. Проверяем акт выполненных работ (для проверки подписи)
                var repairAct = await _context.RepairActs
                    .Include(a => a.Signature)  // 🔥 ДОБАВЬТЕ ЭТУ СТРОКУ!
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.TaskId == diagnosticAct.TaskId);

                if (repairAct == null)
                    return NotFound(new
                    {
                        message = "Акт выполненных работ не найден. Сначала завершите наряды на ремонт.",
                        requestId = requestId,
                        diagnosticActCode = diagnosticAct.ActCode
                    });

                // 🔥 Проверка подписи
                if (repairAct.IsSignedByClient != true)
                    return BadRequest(new
                    {
                        message = "Акт должен быть подписан клиентом перед формированием счёта",
                        isSigned = repairAct.IsSignedByClient
                    });

                // 5. Проверяем, нет ли уже сформированного счёта НА РАБОТЫ
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.RequestId == requestId && i.InvoiceType == "final");

                if (existingInvoice != null)
                {
                    _logger.LogInformation("ℹ️ Счёт на работы уже сформирован: InvoiceId={InvoiceId}", existingInvoice.InvoiceId);
                    return Ok(new InvoiceResponse
                    {
                        InvoiceId = existingInvoice.InvoiceId,
                        Amount = existingInvoice.Amount,
                        FilePath = existingInvoice.FilePath,
                        IsPaid = existingInvoice.Status == InvoiceStatus.Paid,
                        ReceiptPath = existingInvoice.ReceiptPath,
                        CreatedAt = existingInvoice.CreatedAt,
                        Note = "Счёт уже был сформирован ранее"
                    });
                }

                // 6. 🔥 Получаем работы из акта диагностики (используем diagnosticAct.ActCode!)
                var works = await _context.WorkDiagnosticActs
                    .Include(w => w.Work)
                    .Where(w => w.ActCode == diagnosticAct.ActCode && w.IsApprovedByClient == true)
                    .ToListAsync();

                if (!works.Any())
                    return BadRequest(new
                    {
                        message = "Нет согласованных работ для формирования счёта",
                        diagnosticActCode = diagnosticAct.ActCode
                    });

                // 🔥 ИСПРАВЛЕНО: Вычисляем сумму ПЕРЕД использованием
                decimal worksTotal = works.Sum(w =>
                    (w.Work?.PricePerHour ?? 0) * 1
                );

                _logger.LogInformation("💰 Сумма счёта (только работы): {Amount} ₽", worksTotal);

                // 7. Получаем информацию о ЗИП для примечания (тоже через diagnosticAct.ActCode)
                var spares = await _context.SparePartsDiagnosticActs
                    .Include(s => s.Spare)
                    .Where(s => s.ActCode == diagnosticAct.ActCode && s.IsApprovedByClient == true)
                    .ToListAsync();

                decimal prepaidAmount = 0;

                // 8. Генерируем PDF счёта
                var invoiceData = new InvoiceData2
                {
                    RequestId = requestId,
                    DeviceModel = request.Model ?? "",
                    SerialNumber = request.SerialNumber ?? "",
                    FaultDescription = request.Description ?? "",

                    ClientFio = request.ClientEntity?.User.FullName ?? "Неизвестно",
                    ClientPhone = request.ClientEntity?.Phone ?? "",
                    ClientAddress = "",

                    Works = works.Select(w => new WorkInfo2
                    {
                        WorkName = w.Work?.WorkName ?? $"Работа #{w.WorkCode}",
                        WorkCode = w.WorkCode.ToString(),
                        Quantity = 1,
                        TotalPrice = (w.Work?.PricePerHour ?? 0) * 1,
                    }).ToList(),

                    WorksTotal = worksTotal,
                    PrepaidAmount = prepaidAmount,
                    TotalAmount = worksTotal,
                    Currency = "RUB",

                    InvoiceNumber = $"INV-{requestId}-{DateTime.UtcNow:yyyyMMdd}",
                    InvoiceDate = DateTime.UtcNow,
                    PaymentDeadline = DateTime.UtcNow.AddDays(7),

                    CompanyName = "ООО \"СервисЦентр\"",
                    CompanyInn = "7701234567",
                    CompanyKpp = "770101001",
                    CompanyAddress = "123456, г. Москва, ул. Примерная, д. 1",
                    CompanyPhone = "+7 (495) 123-45-67",
                    CompanyEmail = "info@servicecenter.ru",

                    BankAccount = "40702810123456789012",
                    BankName = "ПАО \"Банк Пример\"",
                    BankCorrespondent = "30101810100000000123",
                    BankBik = "044525123",

                    ActReference = $"Акт диагностики №{diagnosticAct.ActCode} от {diagnosticAct.CreatedAt:dd.MM.yyyy}",
                    Note = spares.Any()
                        ? $"ЗИП ({spares?.Count} поз.) оплачен отдельно на этапе предоплаты. Данный счёт — только за выполненные работы."
                        : "Данный счёт — за выполненные работы.",
                    Footer = "Счёт действителен в течение 7 дней с даты выставления"
                };

                // 9. 🔥 Генерируем путь и создаём директорию
                var invoiceFileName = $"Invoice_{requestId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
                var invoiceDirectory = Path.Combine(_env.WebRootPath, "documents", "invoices");
                Directory.CreateDirectory(invoiceDirectory);  // ← Создаём директорию если нет
                var invoiceFullPath = Path.Combine(invoiceDirectory, invoiceFileName);
                var invoiceRelativePath = Path.Combine("documents", "invoices", invoiceFileName);

                // 10. 🔥 Генерируем PDF (передаём ПОЛНЫЙ путь!)
                var generatedPath = await _pdfService.GenerateInvoiceAsync2(
                    requestId,
                    worksTotal,
                    invoiceFullPath  // ← Полный путь к файлу, а не номер счёта!
                );

                // 11. Сохраняем счёт в БД
                var invoice = new InvoiceEntity
                {
                    RequestId = requestId,
                    InvoiceNumber = invoiceData.InvoiceNumber,
                    Amount = worksTotal,
                    FilePath = invoiceRelativePath,  // ← Относительный путь для хранения в БД
                    Status = InvoiceStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    Comment = "Счёт на оплату работ (ЗИП оплачен отдельно)",
                    ReceiptPath = null,
                    InvoiceType = "final"
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Счёт на работы сформирован: InvoiceId={InvoiceId}, Amount={Amount}",
                    invoice.InvoiceId, worksTotal);

                return Ok(new InvoiceResponse
                {
                    InvoiceId = invoice.InvoiceId,
                    Amount = invoice.Amount,
                    FilePath = invoice.FilePath,
                    IsPaid = invoice.Status == InvoiceStatus.Paid,
                    ReceiptPath = invoice.ReceiptPath,
                    CreatedAt = invoice.CreatedAt,
                    Note = "Счёт включает только оплату за работы. ЗИП оплачен отдельно."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка генерации счёта на работы для заявки #{RequestId}", requestId);
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        /// <summary>
        /// Получить информацию о финальном счёте на оплату работ
        /// </summary>
        [HttpGet("{requestId}/invoice")]
        [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvoiceResponse>> GetFinalInvoice(int requestId)
        {
            try
            {
                // Находим финальный счёт для заявки
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.RequestId == requestId && i.InvoiceType == "final");

                if (invoice == null)
                {
                    _logger.LogWarning("⚠️ Финальный счёт для заявки #{RequestId} не найден", requestId);
                    return NotFound(new { message = "Финальный счёт не найден" });
                }

                _logger.LogInformation("✅ Финальный счёт найден: InvoiceId={InvoiceId}, ReceiptPath={ReceiptPath}",
                    invoice.InvoiceId, invoice.ReceiptPath);

                return Ok(new InvoiceResponse
                {
                    InvoiceId = invoice.InvoiceId,
                    Amount = invoice.Amount,
                    FilePath = invoice.FilePath,
                    IsPaid = invoice.Status == InvoiceStatus.Paid,
                    ReceiptPath = invoice.ReceiptPath,
                    ReceiptUploaded = !string.IsNullOrEmpty(invoice.ReceiptPath),
                    CreatedAt = invoice.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка получения финального счёта для заявки #{RequestId}", requestId);
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }

        /// <summary>
        /// Подтвердить оплату финального счёта (за работы)
        /// </summary>
        [HttpPost("invoice/{invoiceId}/confirm-paid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmFinalInvoicePaid(int invoiceId)
        {
            try
            {
                _logger.LogInformation("💰 Подтверждение оплаты финального счёта #{InvoiceId}", invoiceId);

                // Находим счёт
                var invoice = await _context.Invoices
                    .Include(i => i.Request)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice == null)
                {
                    _logger.LogWarning("⚠️ Счёт #{InvoiceId} не найден", invoiceId);
                    return NotFound(new { message = "Счёт не найден" });
                }

                // Проверяем, что это финальный счёт
                if (invoice.InvoiceType != "final")
                {
                    _logger.LogWarning("⚠️ Счёт #{InvoiceId} не является финальным (Тип: {Type})",
                        invoiceId, invoice.InvoiceType);
                    return BadRequest(new
                    {
                        message = "Этот эндпоинт предназначен только для подтверждения оплаты работ",
                        invoiceType = invoice.InvoiceType
                    });
                }

                // Обновляем статус счёта
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidAt = DateTime.UtcNow;

                // Если заявка существует, обновляем её статус
                if (invoice.Request != null)
                {
                    invoice.Request.Status = RequestStatus.Completed;
                    _logger.LogInformation("✅ Заявка #{RequestId} переведена в статус Completed",
                        invoice.Request.Id);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Оплата финального счёта #{InvoiceId} подтверждена", invoiceId);

                return Ok(new
                {
                    message = "Оплата работ подтверждена",
                    invoiceId = invoice.InvoiceId,
                    paidAt = invoice.PaidAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка подтверждения оплаты счёта #{InvoiceId}", invoiceId);
                return StatusCode(500, new { message = "Ошибка сервера", error = ex.Message });
            }
        }


        // 🔥 Вспомогательный класс для запроса
        public class MarkSignedRequest
        {
            public string? SignatureMethod { get; set; }  // "office_handwritten", "photo_upload", "click"
        }

        // 🔥 DTO для ответов


        // 🔥 Ответ для актов
        // Обновите DTO
        public class TaskActsResponse
        {
            public int? ActId { get; set; }  // ← НОВОЕ ПОЛЕ!
            public string? RepairActPath { get; set; }
            public string? SpareWriteOffPath { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class TaskActs
        {
            public string? RepairActPath { get; set; }
            public string? SpareWriteOffPath { get; set; }
            public DateTime? CreatedAt { get; set; }
        }

        public class SpareStatusResult
        {
            public int ActCode { get; set; }
            public bool HasPendingOrders { get; set; }
            public int PendingOrdersCount { get; set; }
            public List<PendingOrderInfo> PendingOrders { get; set; }
        }

        public class PendingOrderInfo
        {
            public string SpareName { get; set; }
            public int Quantity { get; set; }
            public string SupplierName { get; set; }
        }
    }
}
