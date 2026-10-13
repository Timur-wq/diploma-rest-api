//// Diploma.API/Controllers/Client/ClientActController.cs

//using Diploma.DataAccess;
//using Diploma.DataAccess.Entities;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using System.Linq;
//using System.Security.Claims;
//using System.Text.Json;
//using static Diploma.Core.Class1;

//namespace Diploma.API.Controllers.Client
//{
//    [ApiController]
//    [Route("api/client/acts")]
//    [Authorize(Policy = "Authenticated")]
//    public class ClientActController : ControllerBase
//    {
//        private readonly DiplomaDbContext _context;
//        private readonly ILogger<ClientActController> _logger;

//        public ClientActController(DiplomaDbContext context, ILogger<ClientActController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        /// <summary>
//        /// Подписать акт электронной подписью (клик в ЛК) - для ВСЕХ нарядов заявки
//        /// </summary>
//        [HttpPost("{actId}/sign")]
//        public async Task<IActionResult> SignAct(int actId)
//        {
//            try
//            {
//                var clientId = GetCurrentClientId();

//                _logger.LogInformation("✍️ Клиент #{ClientId} подписывает акт #{ActId}", clientId, actId);

//                // 1. Находим акт по ID
//                var act = await _context.RepairActs
//                    .Include(a => a.Task)
//                        .ThenInclude(t => t!.RequestEntity)
//                    .FirstOrDefaultAsync(a => a.Id == actId);

//                if (act == null || act.Task?.RequestEntity?.ClientId != clientId)
//                    return NotFound(new { message = "Акт не найден" });

//                // 2. Находим ВСЕ наряды этой заявки
//                var requestId = act.Task!.RequestId;
//                var taskIds = await _context.TaskEntities
//                    .Where(t => t.RequestId == requestId)
//                    .Select(t => t.WorkOrderId)
//                    .ToListAsync();

//                // 3. Находим ВСЕ акты для этих нарядов
//                var actsToUpdate = await _context.RepairActs
//                    .Where(a => taskIds.Contains(a.TaskId ?? 0))
//                    .ToListAsync();

//                if (!actsToUpdate.Any())
//                    return NotFound(new { message = "Акты не найдены" });

//                // 4. Подписываем ВСЕ акты
//                foreach (var a in actsToUpdate)
//                {
//                    a.IsSignedByClient = true;
//                    a.SignedAt = DateTime.UtcNow;
//                    a.SignatureMethod = "click";
//                    a.ClientVerificationData = JsonSerializer.Serialize(new
//                    {
//                        ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
//                        user_agent = Request.Headers["User-Agent"].ToString(),
//                        signed_at = DateTime.UtcNow,
//                        client_id = clientId
//                    });
//                    _logger.LogInformation("✅ Акт подписан клиентом: ActId={ActId}", a.Id);
//                }

//                await _context.SaveChangesAsync();


//                return Ok(new
//                {
//                    signedAt = DateTime.UtcNow,
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "❌ Ошибка подписания акта клиентом");
//                return StatusCode(500, new { message = "Ошибка сервера" });
//            }
//        }

//        /// <summary>
//        /// Получить статус подписания акта - для ВСЕХ нарядов заявки
//        /// </summary>
//        [HttpGet("{actId}/status")]
//        public async Task<ActionResult<ActSignatureStatus>> GetSignatureStatus(int actId)
//        {
//            try
//            {
//                var clientId = GetCurrentClientId();

//                // 1. Находим акт по ID
//                var act = await _context.RepairActs
//                    .Include(a => a.Task)
//                        .ThenInclude(t => t!.RequestEntity)
//                    .FirstOrDefaultAsync(a => a.Id == actId);

//                if (act == null || act.Task?.RequestEntity?.ClientId != clientId)
//                    return NotFound(new { message = "Акт не найден" });

//                // 2. Проверяем статус ВСЕХ актов этой заявки
//                var requestId = act.Task!.RequestId;
//                var taskIds = await _context.TaskEntities
//                    .Where(t => t.RequestId == requestId)
//                    .Select(t => t.WorkOrderId)
//                    .ToListAsync();

//                var allActs = await _context.RepairActs
//                    .Where(a => taskIds.Contains(a.TaskId ?? 0))
//                    .ToListAsync();

//                // 3. Возвращаем агрегированный статус
//                var allSignedByClient = allActs.All(a => a.IsSignedByClient);
//                var allSignedByDispatcher = allActs.All(a => a.IsSignedByDispatcher);
//                var latestClientSignedAt = allActs.Max(a => a.SignedAt);
//                var latestDispatcherSignedAt = allActs.Max(a => a.DispatcherSignedAt);

//                return Ok(new ActSignatureStatus
//                {
//                    ActId = act.Id,
//                    RequestId = requestId,  // ← Добавлено поле!
//                    IsSignedByClient = allSignedByClient,
//                    ClientSignedAt = latestClientSignedAt,
//                    ClientSignatureMethod = act.SignatureMethod,  // Метод из первого акта
//                    ClientVerificationData = act.ClientVerificationData,
//                    IsSignedByDispatcher = allSignedByDispatcher,
//                    DispatcherSignedAt = latestDispatcherSignedAt,
//                    DispatcherSignedByTabNumber = act.DispatcherSignedByTabNumber,
//                    DispatcherVerificationData = act.DispatcherVerificationData,
//                    IsVerifiedByDispatcher = act.IsVerifiedByDispatcher,
//                    IsFullySigned = allSignedByClient && allSignedByDispatcher,
//                    ActsCount = allActs.Count,
//                    SignedActsCount = allActs.Count(a => a.IsSignedByClient)
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "❌ Ошибка получения статуса подписи");
//                return StatusCode(500, new { message = "Ошибка сервера" });
//            }
//        }
//        [HttpPost("{actId}/client-sign")]
//        public async Task<IActionResult> ClientSignAct(int actId)
//        {
//            var clientId = GetCurrentClientId();

//            // 1. Находим акт по ID
//            var act = await _context.RepairActs
//                .Include(a => a.Task)
//                    .ThenInclude(t => t!.RequestEntity)
//                .FirstOrDefaultAsync(a => a.Id == actId);

//            if (act == null || act.Task?.RequestEntity?.ClientId != clientId)
//                return NotFound(new { message = "Акт не найден" });

//            // 2. Находим ВСЕ наряды этой заявки
//            var requestId = act.Task!.RequestId;
//            var taskIds = await _context.TaskEntities
//                .Where(t => t.RequestId == requestId)
//                .Select(t => t.WorkOrderId)
//                .ToListAsync();

//            // 3. Находим ВСЕ акты для этих нарядов
//            var actsToUpdate = await _context.RepairActs
//                .Where(a => taskIds.Contains(a.TaskId ?? 0))
//                .ToListAsync();

//            if (!actsToUpdate.Any())
//                return NotFound(new { message = "Акты не найдены" });

//            // 4. Подписываем ВСЕ акты
//            foreach (var a in actsToUpdate)
//            {
//                a.IsSignedByClient = true;
//                a.SignedAt = DateTime.UtcNow;
//                a.SignatureMethod = "electronic";
//                a.ClientVerificationData = JsonSerializer.Serialize(new
//                {
//                    ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
//                    user_agent = Request.Headers["User-Agent"].ToString(),
//                    signed_at = DateTime.UtcNow,
//                    client_id = clientId
//                });
//                _logger.LogInformation("✅ Акт подписан клиентом: ActId={ActId}", a.Id);
//            }

//            await _context.SaveChangesAsync();


//            return Ok(new
//            {
//                signedAt = DateTime.UtcNow,
//            });
//        }

//        // 🔥 НОВЫЙ МЕТОД: Получение ID клиента из токена
//        /// <summary>
//        /// Получить ID текущего клиента из JWT-токена
//        /// </summary>
//        private int GetCurrentClientId()
//        {
//            // 🔍 Пробуем несколько вариантов claim для ID клиента
//            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
//                        ?? User.FindFirst("sub")
//                        ?? User.FindFirst("clientId");

//            if (idClaim != null && int.TryParse(idClaim.Value, out var clientId))
//            {
//                _logger.LogDebug("✅ Текущий клиент: ClientId={ClientId}, Login={Login}",
//                    clientId, User.Identity?.Name);
//                return clientId;
//            }

//            // 🔥 Если не нашли ID, логируем и выбрасываем исключение
//            _logger.LogWarning("⚠️ Не удалось найти ID клиента в токене. Claims: {@Claims}",
//                User.Claims.Select(c => new { c.Type, c.Value }));

//            throw new UnauthorizedAccessException("Не удалось определить клиента");
//        }
//    }
//}

// Diploma.API/Controllers/Client/ClientActController.cs

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

namespace Diploma.API.Controllers.Client
{
    [ApiController]
    [Route("api/client/acts")]
    [Authorize(Policy = "Authenticated")]
    public class ClientActController : ControllerBase
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<ClientActController> _logger;

        public ClientActController(DiplomaDbContext context, ILogger<ClientActController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Подписать акт электронной подписью (клик в ЛК) - для ВСЕХ нарядов заявки
        /// </summary>
        [HttpPost("{actId}/sign")]
        public async Task<IActionResult> SignAct(int actId)
        {
            try
            {
                var clientId = GetCurrentClientId();

                _logger.LogInformation("✍️ Клиент #{ClientId} подписывает акт #{ActId}", clientId, actId);

                // 1. Находим акт по ID с загрузкой Signature
                var act = await _context.RepairActs
                    .Include(a => a.Signature)  // 🔥 Загружаем подпись!
                    .Include(a => a.Task)
                        .ThenInclude(t => t!.RequestEntity)
                    .FirstOrDefaultAsync(a => a.Id == actId);

                if (act == null || act.Task?.RequestEntity?.ClientId != clientId)
                    return NotFound(new { message = "Акт не найден" });

                // 2. Находим ВСЕ наряды этой заявки
                var requestId = act.Task!.RequestId;
                var taskIds = await _context.TaskEntities
                    .Where(t => t.RequestId == requestId)
                    .Select(t => t.WorkOrderId)
                    .ToListAsync();

                // 3. Находим ВСЕ акты для этих нарядов с Signature
                var actsToUpdate = await _context.RepairActs
                    .Include(a => a.Signature)  // 🔥 Загружаем подпись для каждого акта!
                    .Where(a => taskIds.Contains(a.TaskId ?? 0))
                    .ToListAsync();

                if (!actsToUpdate.Any())
                    return NotFound(new { message = "Акты не найдены" });

                // 4. Подписываем ВСЕ акты через свойства-обёртки (они работают с Signature)
                foreach (var a in actsToUpdate)
                {
                    // 🔥 Используем свойства-обёртки — они автоматически создадут Signature, если нет
                    a.IsSignedByClient = true;
                    a.SignedAt = DateTime.UtcNow;
                    a.SignatureMethod = "click";
                    a.ClientVerificationData = JsonSerializer.Serialize(new
                    {
                        ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        user_agent = Request.Headers["User-Agent"].ToString(),
                        signed_at = DateTime.UtcNow,
                        client_id = clientId
                    });
                    _logger.LogInformation("✅ Акт подписан клиентом: ActId={ActId}, SignatureId={SigId}",
                        a.Id, a.Signature?.Id);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    signedAt = DateTime.UtcNow,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка подписания акта клиентом");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить статус подписания акта - для ВСЕХ нарядов заявки
        /// </summary>
        [HttpGet("{actId}/status")]
        public async Task<ActionResult<ActSignatureStatus>> GetSignatureStatus(int actId)
        {
            try
            {
                var clientId = GetCurrentClientId();

                // 1. Находим акт по ID с загрузкой Signature
                var act = await _context.RepairActs
                    .Include(a => a.Signature)  // 🔥 Загружаем подпись!
                    .Include(a => a.Task)
                        .ThenInclude(t => t!.RequestEntity)
                    .FirstOrDefaultAsync(a => a.Id == actId);

                if (act == null || act.Task?.RequestEntity?.ClientId != clientId)
                    return NotFound(new { message = "Акт не найден" });

                // 2. Проверяем статус ВСЕХ актов этой заявки с Signature
                var requestId = act.Task!.RequestId;
                var taskIds = await _context.TaskEntities
                    .Where(t => t.RequestId == requestId)
                    .Select(t => t.WorkOrderId)
                    .ToListAsync();

                var allActs = await _context.RepairActs
                    .Include(a => a.Signature)  // 🔥 Загружаем подпись для агрегации!
                    .Where(a => taskIds.Contains(a.TaskId ?? 0))
                    .ToListAsync();

                // 3. Возвращаем агрегированный статус через свойства-обёртки
                var allSignedByClient = allActs.All(a => a.IsSignedByClient);  // 🔥 Читает из Signature
                var allSignedByDispatcher = allActs.All(a => a.IsSignedByDispatcher);
                var latestClientSignedAt = allActs.Max(a => a.SignedAt);
                var latestDispatcherSignedAt = allActs.Max(a => a.DispatcherSignedAt);

                return Ok(new ActSignatureStatus
                {
                    ActId = act.Id,
                    RequestId = requestId,
                    IsSignedByClient = allSignedByClient,
                    ClientSignedAt = latestClientSignedAt,
                    ClientSignatureMethod = act.SignatureMethod,  // 🔥 Читает из Signature
                    ClientVerificationData = act.ClientVerificationData,
                    IsSignedByDispatcher = allSignedByDispatcher,
                    DispatcherSignedAt = latestDispatcherSignedAt,
                    DispatcherSignedByTabNumber = act.DispatcherSignedByTabNumber,
                    DispatcherVerificationData = act.DispatcherVerificationData,
                    IsVerifiedByDispatcher = act.IsVerifiedByDispatcher,
                    IsFullySigned = allSignedByClient && allSignedByDispatcher,
                    ActsCount = allActs.Count,
                    SignedActsCount = allActs.Count(a => a.IsSignedByClient)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка получения статуса подписи");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("{actId}/client-sign")]
        public async Task<IActionResult> ClientSignAct(int actId)
        {
            var clientId = GetCurrentClientId();

            // 1. Находим акт по ID с Signature
            var act = await _context.RepairActs
                .Include(a => a.Signature)  // 🔥 Загружаем подпись!
                .Include(a => a.Task)
                    .ThenInclude(t => t!.RequestEntity)
                .FirstOrDefaultAsync(a => a.Id == actId);

            if (act == null || act.Task?.RequestEntity?.ClientId != clientId)
                return NotFound(new { message = "Акт не найден" });

            // 2. Находим ВСЕ наряды этой заявки
            var requestId = act.Task!.RequestId;
            var taskIds = await _context.TaskEntities
                .Where(t => t.RequestId == requestId)
                .Select(t => t.WorkOrderId)
                .ToListAsync();

            // 3. Находим ВСЕ акты для этих нарядов с Signature
            var actsToUpdate = await _context.RepairActs
                .Include(a => a.Signature)  // 🔥 Загружаем подпись!
                .Where(a => taskIds.Contains(a.TaskId ?? 0))
                .ToListAsync();

            if (!actsToUpdate.Any())
                return NotFound(new { message = "Акты не найдены" });

            // 4. Подписываем ВСЕ акты через свойства-обёртки
            foreach (var a in actsToUpdate)
            {
                a.IsSignedByClient = true;
                a.SignedAt = DateTime.UtcNow;
                a.SignatureMethod = "electronic";
                a.ClientVerificationData = JsonSerializer.Serialize(new
                {
                    ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    user_agent = Request.Headers["User-Agent"].ToString(),
                    signed_at = DateTime.UtcNow,
                    client_id = clientId
                });
                _logger.LogInformation("✅ Акт подписан клиентом: ActId={ActId}, SignatureId={SigId}",
                    a.Id, a.Signature?.Id);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                signedAt = DateTime.UtcNow,
            });
        }

        /// <summary>
        /// Получить ID текущего клиента из JWT-токена
        /// </summary>
        private int GetCurrentClientId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub")
                        ?? User.FindFirst("clientId");

            if (idClaim != null && int.TryParse(idClaim.Value, out var clientId))
            {
                _logger.LogDebug("✅ Текущий клиент: ClientId={ClientId}, Login={Login}",
                    clientId, User.Identity?.Name);
                return clientId;
            }

            _logger.LogWarning("⚠️ Не удалось найти ID клиента в токене. Claims: {@Claims}",
                User.Claims.Select(c => new { c.Type, c.Value }));

            throw new UnauthorizedAccessException("Не удалось определить клиента");
        }
    }
}