// Diploma.API/Controllers/Client/ClientContractController.cs
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace Diploma.API.Controllers.Client
{
    [ApiController]
    [Route("api/client/contract")]
    [Authorize(Policy = "Authenticated")]
    public class ClientContractController : ControllerBase
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<ClientContractController> _logger;

        public ClientContractController(DiplomaDbContext context, ILogger<ClientContractController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int GetCurrentClientId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        /// <summary>
        /// Получить договор для заявки
        /// </summary>
        [HttpGet("request/{requestId}")]
        public async Task<ActionResult<ContractInfoDto>> GetContract(int requestId)
        {
            var clientId = GetCurrentClientId();

            var contract = await _context.Contracts
                .Include(c => c.Signature)
                .Include(c => c.Request)
                .FirstOrDefaultAsync(c => c.RequestId == requestId && c.Request.ClientId == clientId);

            if (contract == null)
                return NotFound(new { message = "Договор не найден" });

            return Ok(new ContractInfoDto
            {
                ContractId = contract.ContractId,
                ContractNumber = contract.ContractNumber,
                FilePath = contract.FilePath,
                // 🔥 ИСПРАВЛЕНО: Status убран из ContractEntity
                Status = contract.IsFullySigned ? "Signed" : "Draft",
                // 🔥 ИСПРАВЛЕНО: берём из Signature
                IsSignedByClient = contract.Signature?.IsSignedByClient ?? false,
                IsSignedByDispatcher = contract.Signature?.IsSignedByDispatcher ?? false,
                IsFullySigned = contract.IsFullySigned,
                SignedAt = contract.Signature?.SignedAt,
                DispatcherSignedAt = contract.Signature?.DispatcherSignedAt
            });
        }

        /// <summary>
        /// Подписать договор кликом в ЛК
        /// </summary>
        [HttpPost("{contractId}/sign")]
        public async Task<IActionResult> SignContract(int contractId)
        {
            try
            {
                var clientId = GetCurrentClientId();

                var contract = await _context.Contracts
                    .Include(c => c.Request)
                    .Include(c => c.Signature)
                    .FirstOrDefaultAsync(c => c.ContractId == contractId && c.Request.ClientId == clientId);

                if (contract == null)
                    return NotFound(new { message = "Договор не найден" });

                // 🔥 Если подписи нет — создаём
                if (contract.Signature == null)
                {
                    contract.Signature = new SignatureEntity
                    {
                        Contract = contract
                    };
                    _context.Signatures.Add(contract.Signature);
                }

                // 🔥 Подписываем через SignatureEntity
                contract.Signature.IsSignedByClient = true;
                contract.Signature.SignedAt = DateTime.UtcNow;
                contract.Signature.SignatureMethod = "click";
                contract.Signature.ClientVerificationData = JsonSerializer.Serialize(new
                {
                    ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    user_agent = Request.Headers["User-Agent"].ToString(),
                    signed_at = DateTime.UtcNow,
                    client_id = clientId
                });

                // 🔥 Для обратной совместимости обновляем и поля в Contract
                //contract.IsSignedByClient = true;
                //contract.SignedAt = contract.Signature.SignedAt;

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Клиент #{ClientId} подписал договор #{ContractId}", clientId, contractId);

                return Ok(new
                {
                    message = "Договор подписан",
                    signedAt = contract.Signature.SignedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка подписания договора");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Скачать договор
        /// </summary>
        [HttpGet("{contractId}/download")]
        public async Task<IActionResult> DownloadContract(int contractId)
        {
            var clientId = GetCurrentClientId();

            var contract = await _context.Contracts
                .Include(c => c.Request)
                .FirstOrDefaultAsync(c => c.ContractId == contractId && c.Request.ClientId == clientId);

            if (contract == null || string.IsNullOrEmpty(contract.FilePath))
                return NotFound(new { message = "Договор не найден" });

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", contract.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = "Файл не найден" });

            return PhysicalFile(fullPath, "application/pdf", $"Contract_{contractId}.pdf");
        }
    }

    public class ContractInfoDto
    {
        public int ContractId { get; set; }
        public string? ContractNumber { get; set; }
        public string? FilePath { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsSignedByClient { get; set; }
        public bool IsSignedByDispatcher { get; set; }
        public bool IsFullySigned { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? DispatcherSignedAt { get; set; }
    }
}