using Diploma.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Diploma.Core.Class1;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Diploma.DataAccess;

namespace Diploma.API.Controllers
{
    [ApiController]
    //[Route("api/client/[controller]")]
    [Route("api/client/DiagnosticAct")]
    [Authorize(Roles = "Client")]
    public class DiagnosticActClientController : ControllerBase
    {
        private readonly IDiagnosticActService _actService;
        private readonly ILogger<DiagnosticActController> _logger;
        private readonly DiplomaDbContext _context;

        public DiagnosticActClientController(
            IDiagnosticActService actService,
            ILogger<DiagnosticActController> logger,
            DiplomaDbContext dbContext)
        {
            _actService = actService;
            _logger = logger;
            _context = dbContext;
        }

        private int GetCurrentClientId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        /// <summary>
        /// Получить акт для просмотра и согласования
        /// </summary>
        [HttpGet("{actCode}")]
        [ProducesResponseType(typeof(DiagnosticActDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DiagnosticActDto>> GetAct(int actCode)
        {
            try
            {
                var clientId = GetCurrentClientId();

                if (clientId == 0)
                {
                    _logger.LogWarning("❌ Не удалось определить ID клиента");
                    return Unauthorized(new { message = "Не удалось определить пользователя" });
                }

                _logger.LogInformation("👤 Клиент #{ClientId} запрашивает акт #{ActCode}", clientId, actCode);

                var act = await _actService.GetActForClientAsync(actCode, clientId);

                if (act == null)
                {
                    // 🔥 Подробная диагностика
                    var rawAct = await _context.DiagnosticActs
                        .Include(a => a.Task)
                        .FirstOrDefaultAsync(a => a.ActCode == actCode);

                    if (rawAct == null)
                    {
                        _logger.LogWarning("⚠️ Акт #{ActCode} не найден в БД", actCode);
                        return NotFound(new { message = $"Акт #{actCode} не найден" });
                    }

                    if (rawAct.Task?.RequestId == null)
                    {
                        _logger.LogWarning("⚠️ У акта #{ActCode} нет связи с заявкой", actCode);
                        return NotFound(new { message = "Акт не связан с заявкой" });
                    }

                    var req = await _context.Requests
                        .FirstOrDefaultAsync(r => r.Id == rawAct.Task.RequestId);

                    if (req?.ClientId != clientId)
                    {
                        _logger.LogWarning("⚠️ Акт #{ActCode} принадлежит клиенту #{OwnerId}, а не #{ClientId}",
                            actCode, req?.ClientId, clientId);
                        return NotFound(new { message = "Акт не найден или недоступен" });
                    }

                    _logger.LogWarning("⚠️ Акт #{ActCode} имеет статус {Status}, недоступен для клиента",
                        actCode, rawAct.Status);
                    return NotFound(new { message = $"Акт ещё не отправлен на согласование (статус: {rawAct.Status})" });
                }


                return Ok(act);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении акта для клиента");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Отправить решение по акту (согласование/отказ работ)
        /// </summary>
        [HttpPost("{actCode}/decision")]
        [ProducesResponseType(typeof(ActDecisionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ActDecisionResponseDto>> SubmitDecision(
            int actCode,
            [FromBody] ClientActDecisionRequest request)
        {
            try
            {
                var clientId = GetCurrentClientId();

                if (clientId == 0)
                    return Unauthorized(new { message = "Не удалось определить пользователя" });

                _logger.LogInformation("✅ Клиент #{ClientId} отправляет решение по акту #{ActCode}", clientId, actCode);

                var result = await _actService.SubmitClientDecisionAsync(actCode, clientId, request);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                _logger.LogInformation("✅ Решение клиента обработано: {Status}", result.NewStatus);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при обработке решения клиента");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить список актов клиента, ожидающих согласования
        /// </summary>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(List<DiagnosticActDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<DiagnosticActDto>>> GetPendingActs()
        {
            try
            {
                var clientId = GetCurrentClientId();

                if (clientId == 0)
                    return Unauthorized(new { message = "Не удалось определить пользователя" });

                // Получаем все акты, связанные с заявками этого клиента
                var acts = await _actService.GetActsForClientPendingAsync(clientId);
                return Ok(acts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении ожидающих актов");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }
}
