// Diploma.API/Controllers/DispatcherDiagnosticActController.cs

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Diploma.Application.Services;
using Diploma.Core.Enums;
using Diploma.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using static Diploma.Core.Class1;
using Diploma.DataAccess;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/dispatcher/[controller]")]
    [Authorize(Roles = "Dispatcher")]
    public class DiagnosticActController : ControllerBase
    {
        private readonly IDiagnosticActService _actService;
        private readonly ILogger<DiagnosticActController> _logger;
        private readonly DiplomaDbContext _context;

        public DiagnosticActController(IDiagnosticActService actService, ILogger<DiagnosticActController> logger, DiplomaDbContext diplomaDbContext)
        {
            _actService = actService;
            _logger = logger;
            _context = diplomaDbContext;
        }

        /// <summary>
        /// Получить акт диагностики по заявке
        /// </summary>
        [HttpGet("by-request/{requestId}")]
        [ProducesResponseType(typeof(DiagnosticActDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DiagnosticActDto>> GetActByRequestId(int requestId)
        {
            try
            {
                // Находим наряд по заявке
                // (нужно добавить метод в сервис или сделать напрямую через DbContext)
                var acts = await _actService.GetActsForDispatcherAsync(requestId: requestId);

                if (acts == null || acts.Count == 0)
                    return NotFound(new { message = "Акт диагностики не найден" });

                return Ok(acts.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении акта по заявке #{RequestId}", requestId);
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить список актов для диспетчера
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<DiagnosticActDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DiagnosticActDto>>> GetActs(
            [FromQuery] int? requestId,
            [FromQuery] DiagnosticActStatus? status)
        {
            try
            {
                var acts = await _actService.GetActsForDispatcherAsync(requestId, status);
                return Ok(acts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка актов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Отправить акт клиенту на согласование
        /// </summary>
        [HttpPost("{actCode}/send-to-client")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendToClient(int actCode, [FromBody] SubmitActToClientRequest request)
        {
            try
            {
                var dispatcherTabNum = GetCurrentDispatcherTabNum();

                var success = await _actService.SubmitActToClientAsync(actCode, dispatcherTabNum, request);

                if (!success)
                    return BadRequest(new { message = "Не удалось отправить акт клиенту" });

                // 🔥 Обновляем статус заявки на "WaitingForClientApproval"
                await UpdateRequestStatusAsync(actCode, RequestStatus.WaitingForClientApproval);

                return Ok(new { message = "Акт отправлен клиенту на согласование" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке акта клиенту");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Запустить жадный алгоритм оптимизации закупок ЗИП
        /// </summary>
        [HttpPost("{actCode}/optimize-procurement")]
        [ProducesResponseType(typeof(ProcurementResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProcurementResultDto>> OptimizeProcurement(int actCode)
        {
            try
            {
                _logger.LogInformation("🤖 Диспетчер запросил оптимизацию закупок для акта #{ActCode}", actCode);

                // Проверяем, что акт существует и доступен диспетчеру
                var act = await _context.DiagnosticActs
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.ActCode == actCode);

                if (act == null)
                    return NotFound(new { message = "Акт не найден" });

                // Запускаем алгоритм
                var optimizationService = HttpContext.RequestServices.GetRequiredService<IProcurementOptimizationService>();
                var result = await optimizationService.OptimizeProcurementAsync(actCode);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при оптимизации закупок для акта #{ActCode}", actCode);
                return StatusCode(500, new { message = "Ошибка при выполнении алгоритма" });
            }
        }

        private int GetCurrentDispatcherTabNum()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null && int.TryParse(claim.Value, out var tabNum) ? tabNum : 0;
        }

        // 🔥 Вспомогательный метод для обновления статуса заявки
        private async Task UpdateRequestStatusAsync(int actCode, RequestStatus newStatus)
        {
            // Этот метод можно вынести в сервис или оставить в контроллере
            var act = await _context.DiagnosticActs
                .Include(a => a.Task)
                    .ThenInclude(t => t!.RequestEntity)
                .FirstOrDefaultAsync(a => a.ActCode == actCode);

            if (act?.Task?.RequestEntity != null)
            {
                act.Task.RequestEntity.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }
    }
}