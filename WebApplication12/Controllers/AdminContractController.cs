// Diploma.API/Controllers/Admin/AdminContractController.cs
using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;
using static Diploma.Application.Services.PdfGenerationService;

namespace Diploma.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/contract")]
    [Authorize(Policy = "Authenticated")]
    public class AdminContractController : ControllerBase
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<AdminContractController> _logger;
        private readonly IPdfGenerationService _pdfService;
        private readonly IWebHostEnvironment _env;

        public AdminContractController(
            DiplomaDbContext context,
            ILogger<AdminContractController> logger,
            IPdfGenerationService pdfService,
            IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _pdfService = pdfService;
            _env = env;
        }

        /// <summary>
        /// Диспетчер подписывает договор
        /// </summary>
        [HttpPost("{contractId}/sign")]
        public async Task<IActionResult> SignContract(int contractId)
        {
            try
            {
                var dispatcherTab = GetCurrentDispatcherTabNumber();

                var contract = await _context.Contracts
                    .Include(c => c.Signature)
                    .FirstOrDefaultAsync(c => c.ContractId == contractId);

                if (contract == null)
                    return NotFound(new { message = "Договор не найден" });

                // 🔥 Если подписи нет — создаём
                if (contract.Signature == null)
                {
                    contract.Signature = new SignatureEntity
                    {
                        Contract = contract
                        // CreatedAt будет установлен по умолчанию в сущности
                    };
                    _context.Signatures.Add(contract.Signature);
                }

                // 🔥 Проверяем, что клиент уже подписал (через SignatureEntity)
                if (contract.Signature.IsSignedByClient != true)
                    return BadRequest(new { message = "Сначала клиент должен подписать договор" });

                // 🔥 Подписываем диспетчером через SignatureEntity
                contract.Signature.IsSignedByDispatcher = true;
                contract.Signature.DispatcherSignedAt = DateTime.UtcNow;
                contract.Signature.DispatcherSignedByTabNumber = dispatcherTab;
                contract.Signature.DispatcherVerificationData = JsonSerializer.Serialize(new
                {
                    dispatcher_tab = dispatcherTab,
                    ip = HttpContext.Connection.RemoteIpAddress != null ? HttpContext.Connection.RemoteIpAddress.ToString() : null,
                    user_agent = Request.Headers["User-Agent"].ToString(),
                    signed_at = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ Диспетчер #{TabNum} подписал договор #{ContractId}", dispatcherTab, contractId);

                return Ok(new
                {
                    message = "Договор подписан диспетчером",
                    signedAt = contract.Signature.DispatcherSignedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка подписания договора диспетчером");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Перегенерировать договор с визуальными подписями
        /// </summary>
        [HttpPost("{contractId}/regenerate-with-signatures")]
        public async Task<IActionResult> RegenerateContractWithSignatures(int contractId)
        {
            try
            {
                var contract = await _context.Contracts
                    .Include(c => c.Request)
                        .ThenInclude(r => r.ClientEntity)
                    .Include(c => c.Signature)
                    .FirstOrDefaultAsync(c => c.ContractId == contractId);

                if (contract == null)
                    return NotFound(new { message = "Договор не найден" });

                var signature = contract.Signature;

                // 🔥 Проверяем, что обе стороны подписали
                bool clientSigned = signature != null && signature.IsSignedByClient == true;
                bool dispatcherSigned = signature != null && signature.IsSignedByDispatcher == true;

                if (!clientSigned || !dispatcherSigned)
                    return BadRequest(new
                    {
                        message = "Договор должен быть подписан обеими сторонами",
                        isSignedByClient = clientSigned,
                        isSignedByDispatcher = dispatcherSigned
                    });

                // 🔥 Получаем данные диспетчера
                var dispatcher = await _context.Stafs
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.TabNum == (signature != null ? signature.DispatcherSignedByTabNumber : 0));

                // 🔥 Генерируем договор с подписями
                var pdfData = new Core.Class1.ContractPdfData
                {
                    RequestId = contract.RequestId,
                    ContractNumber = contract.ContractNumber,
                    ClientFio = contract.Request != null && contract.Request.ClientEntity != null && contract.Request.ClientEntity.User != null
                        ? contract.Request.ClientEntity.User.FullName
                        : "Неизвестно",
                    ClientPhone = contract.Request != null && contract.Request.ClientEntity != null
                        ? contract.Request.ClientEntity.Phone
                        : "",
                    DeviceModel = contract.Request != null ? contract.Request.Model : "",
                    SerialNumber = contract.Request != null ? contract.Request.SerialNumber : "",
                    Description = contract.Request != null ? contract.Request.Description : "",
                    CreatedAt = contract.CreatedAt,

                    // 🔥 Данные подписей из SignatureEntity
                    ClientSignedAt = signature != null ? signature.SignedAt : (DateTime?)null,
                    ClientSignatureMethod = signature != null ? signature.SignatureMethod : null,
                    ClientIp = signature != null && signature.ClientVerificationData != null
                        ? ExtractIpFromVerificationData(signature.ClientVerificationData)
                        : null,

                    DispatcherSignedAt = signature != null ? signature.DispatcherSignedAt : (DateTime?)null,
                    DispatcherFio = dispatcher != null && dispatcher.User != null ? dispatcher.User.FullName : "Неизвестно",
                    DispatcherTabNumber = signature != null ? signature.DispatcherSignedByTabNumber : (int?)null
                };

                var contractPath = await _pdfService.GenerateContractWithSignaturesAsync(pdfData);

                // 🔥 Обновляем путь к файлу
                contract.FilePath = contractPath;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Договор перегенерирован с подписями",
                    contractPath
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка перегенерации договора");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить статус подписания договора
        /// </summary>
        [HttpGet("{contractId}/signature-status")]
        public async Task<ActionResult<ContractSignatureStatus>> GetSignatureStatus(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Signature)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null)
                return NotFound(new { message = "Договор не найден" });

            var signature = contract.Signature;

            return Ok(new ContractSignatureStatus
            {
                ContractId = contractId,
                IsSignedByClient = signature != null && signature.IsSignedByClient == true,
                ClientSignedAt = signature != null ? signature.SignedAt : (DateTime?)null,
                ClientSignatureMethod = signature != null ? signature.SignatureMethod : null,
                IsSignedByDispatcher = signature != null && signature.IsSignedByDispatcher == true,
                DispatcherSignedAt = signature != null ? signature.DispatcherSignedAt : (DateTime?)null,
                DispatcherSignedByTabNumber = signature != null ? signature.DispatcherSignedByTabNumber : (int?)null,
                IsFullySigned = contract.IsFullySigned
            });
        }

        private int GetCurrentDispatcherTabNumber()
        {
            var tabClaim = User.FindFirst("tabNumber")
                        ?? User.FindFirst("TabNum")
                        ?? User.FindFirst(ClaimTypes.NameIdentifier)
                        ?? User.FindFirst("sub");

            if (tabClaim != null && int.TryParse(tabClaim.Value, out var tabNumber))
                return tabNumber;

            return 0;
        }

        // 🔥 Вспомогательный метод для извлечения IP из JSON
        private string? ExtractIpFromVerificationData(string verificationData)
        {
            try
            {
                using var doc = JsonDocument.Parse(verificationData);
                var ipElement = doc.RootElement.GetProperty("ip");
                return ipElement.GetString();
            }
            catch
            {
                return null;
            }
        }
    }

    public class ContractSignatureStatus
    {
        public int ContractId { get; set; }
        public bool IsSignedByClient { get; set; }
        public DateTime? ClientSignedAt { get; set; }
        public string? ClientSignatureMethod { get; set; }
        public bool IsSignedByDispatcher { get; set; }
        public DateTime? DispatcherSignedAt { get; set; }
        public int? DispatcherSignedByTabNumber { get; set; }
        public bool IsFullySigned { get; set; }
    }
}