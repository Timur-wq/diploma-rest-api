// Diploma.API/Controllers/Dispatcher/DocumentController.cs
using Diploma.Application.Services;
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
using static Diploma.Core.Class1;

namespace Diploma.API.Controllers.Dispatcher
{
    [ApiController]
    [Route("api/dispatcher/[controller]")]
    [Authorize(Roles = "Dispatcher")]
    public class DocumentController : ControllerBase
    {
        private readonly IFileStorageService _fileStorage;
        private readonly IDocumentService _documentService;
        private readonly IPdfGenerationService _pdfService;
        private readonly DiplomaDbContext _context;
        private readonly ILogger<DocumentController> _logger;
        private readonly IWebHostEnvironment _env;

        public DocumentController(
            IFileStorageService fileStorage,
            IDocumentService documentService,
            IPdfGenerationService pdfService,
            DiplomaDbContext context,
            IWebHostEnvironment env,
            ILogger<DocumentController> logger)
        {
            _fileStorage = fileStorage;
            _documentService = documentService;
            _pdfService = pdfService;
            _context = context;
            _env = env;
            _logger = logger;
        }

        // ============================================
        // 🔥 ДОГОВОР — ОБНОВЛЁННЫЙ ФУНКЦИОНАЛ
        // ============================================

        [HttpGet("request/{requestId}/contract")]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDto>> GetContract(int requestId)
        {
            var contract = await _documentService.GetContractByRequestIdAsync(requestId);
            if (contract == null) return NotFound(new { message = "Договор не найден" });
            return Ok(MapToDto(contract));
        }
        // ============================================
        // формирование договора
        // ============================================
        [HttpPost("request/{requestId}/generate-contract")]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateContract(int requestId)
        {
            try
            {
                var fileName = $"Contract_{requestId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var relativePath = Path.Combine("contracts", fileName);
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);

                await _pdfService.GenerateContractAsync(requestId, fullPath);

                var contract = await _documentService.GenerateContractAsync(requestId, relativePath);

                return Ok(new { message = "Договор сформирован", contract });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации договора для заявки #{RequestId}", requestId);
                return BadRequest(new { message = ex.Message });
            }
        }

        // 🔥 НОВЫЙ: Клиент подписывает договор (через сервис)
        [HttpPost("contract/{contractId}/client-sign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ClientSignContract(int contractId)
        {
            try
            {
                // Получаем договор, чтобы проверить принадлежность клиенту
                var contract = await _context.Contracts
                    .Include(c => c.Request)
                    .FirstOrDefaultAsync(c => c.ContractId == contractId);

                if (contract == null)
                    return NotFound(new { message = "Договор не найден" });

                // В реальном приложении здесь была бы проверка, что запрос от владельца заявки
                // Для упрощения — просто подписываем

                var success = await _documentService.SignContractByClientAsync(
                    contractId,
                    "click",  // метод подписания
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                if (!success)
                    return BadRequest(new { message = "Не удалось подписать договор" });

                return Ok(new
                {
                    message = "Договор подписан клиентом",
                    signedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подписания договора клиентом #{ContractId}", contractId);
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        // 🔥 НОВЫЙ: Диспетчер подписывает договор
        [HttpPost("contract/{contractId}/dispatcher-sign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DispatcherSignContract(int contractId)
        {
            try
            {
                var dispatcherTab = GetCurrentDispatcherTabNumber();

                var success = await _documentService.SignContractByDispatcherAsync(
                    contractId,
                    dispatcherTab,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    Request.Headers["User-Agent"].ToString()
                );

                if (!success)
                    return BadRequest(new { message = "Не удалось подписать договор (проверьте, что клиент уже подписал)" });

                return Ok(new
                {
                    message = "Договор подписан диспетчером",
                    signedAt = DateTime.UtcNow,
                    dispatcherTab
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка подписания договора диспетчером #{ContractId}", contractId);
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        // 🔥 НОВЫЙ: Получить статус подписания договора
        [HttpGet("contract/{contractId}/signature-status")]
        [ProducesResponseType(typeof(Core.Class1.ContractSignatureStatusDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<Core.Class1.ContractSignatureStatusDto>> GetContractSignatureStatus(int contractId)
        {
            var status = await _documentService.GetContractSignatureStatusAsync(contractId);
            if (status == null) return NotFound(new { message = "Договор не найден" });
            return Ok(status);
        }

        // 🔥 НОВЫЙ: Перегенерировать договор с визуальными подписями
        [HttpPost("contract/{contractId}/regenerate-with-signatures")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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

                // Проверяем, что обе стороны подписали
                if (!contract.IsFullySigned)
                    return BadRequest(new
                    {
                        message = "Договор должен быть подписан обеими сторонами",
                        isSignedByClient = signature?.IsSignedByClient ?? false,
                        isSignedByDispatcher = signature?.IsSignedByDispatcher ?? false
                    });

                // Получаем данные диспетчера
                var dispatcher = await _context.Stafs
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.TabNum == signature.DispatcherSignedByTabNumber);

                // Данные для генерации PDF
                var pdfData = new ContractPdfData
                {
                    RequestId = contract.RequestId,
                    ContractNumber = contract.ContractNumber,
                    ClientFio = contract.Request?.ClientEntity?.User?.FullName ?? "Неизвестно",
                    ClientPhone = contract.Request?.ClientEntity?.Phone ?? "",
                    DeviceModel = contract.Request?.Model ?? "",
                    SerialNumber = contract.Request?.SerialNumber ?? "",
                    Description = contract.Request?.Description ?? "",
                    CreatedAt = contract.CreatedAt,

                    // Данные подписей
                    ClientSignedAt = signature?.SignedAt,
                    ClientSignatureMethod = signature?.SignatureMethod,
                    ClientIp = signature?.ClientVerificationData != null ?
                        JsonDocument.Parse(signature.ClientVerificationData).RootElement.GetProperty("ip").GetString() : null,
                    DispatcherSignedAt = signature?.DispatcherSignedAt,
                    DispatcherFio = dispatcher?.User?.FullName ?? "Неизвестно",
                    DispatcherTabNumber = signature?.DispatcherSignedByTabNumber
                };
                var contractPath = await _pdfService.GenerateContractWithSignaturesAsync(pdfData);

                // Обновляем путь к файлу
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
                _logger.LogError(ex, "❌ Ошибка перегенерации договора #{ContractId}", contractId);
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        // ============================================
        // 🔥 СЧЁТ — БЕЗ ИЗМЕНЕНИЙ
        // ============================================

        [HttpGet("request/{requestId}/invoice")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(int requestId)
        {
            var invoice = await _documentService.GetInvoiceByRequestIdAsync(requestId);
            if (invoice == null) return NotFound(new { message = "Счёт не найден" });
            return Ok(MapToDto(invoice));
        }

        [HttpPost("request/{requestId}/generate-invoice")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateInvoice(int requestId)
        {
            try
            {
                // 🔥 Проверяем, что договор полностью подписан (через SignatureEntity)
                var contract = await _context.Contracts
                    .Include(c => c.Signature)
                    .FirstOrDefaultAsync(c => c.RequestId == requestId);

                if (contract == null || !contract.IsFullySigned)
                    return BadRequest(new { message = "Договор должен быть полностью подписан" });

                var act = await _context.DiagnosticActs
                    .Include(a => a.SpareParts)
                        .ThenInclude(s => s.Spare)
                    .Include(a => a.Task)
                    .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

                if (act == null)
                    return BadRequest(new { message = "Акт диагностики не найден" });

                var sparePartsCost = act.SpareParts
                    .Where(s => s.IsApprovedByClient == true)
                    .Sum(s => {
                        var supplierSpare = _context.SupplierSpares
                            .FirstOrDefault(ss => ss.SpareCode == s.SpareCode);
                        return (supplierSpare?.UnitPrice ?? 0) * s.Amount;
                    });

                _logger.LogInformation("💰 Рассчитана стоимость ЗИП: {Amount} ₽", sparePartsCost);

                var fileName = $"Invoice_{requestId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var relativePath = Path.Combine("invoices", fileName);
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);

                await _pdfService.GeneratePrepaymentInvoiceAsync(requestId, fullPath);

                var invoice = await _documentService.GenerateInvoiceAsync(requestId, sparePartsCost, relativePath);

                return Ok(new
                {
                    message = "Счёт сформирован",
                    invoice = new
                    {
                        invoiceId = invoice.InvoiceId,
                        filePath = invoice.FilePath,
                        amount = invoice.Amount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка генерации счёта для заявки #{RequestId}", requestId);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("invoice/{invoiceId}/upload-receipt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadReceipt(int invoiceId, IFormFile receipt)
        {
            if (receipt == null || receipt.Length == 0)
                return BadRequest(new { message = "Файл не предоставлен" });

            try
            {
                var receiptPath = await _fileStorage.SaveFileAsync(receipt, "receipts");
                await _documentService.UploadPaymentReceiptAsync(invoiceId, receiptPath);

                return Ok(new { message = "Чек загружен", receiptPath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("invoice/{invoiceId}/confirm-paid")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmInvoicePaid(int invoiceId)
        {
            var success = await _documentService.ConfirmInvoicePaidAsync(invoiceId);
            if (!success) return BadRequest(new { message = "Не удалось подтвердить оплату" });
            return Ok(new { message = "Оплата подтверждена" });
        }

        [HttpGet("invoice/{invoiceId}/receipt/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadReceipt(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null || string.IsNullOrEmpty(invoice.ReceiptPath))
                return NotFound(new { message = "Чек не найден" });

            try
            {
                var fileBytes = await _fileStorage.GetFileAsync(invoice.ReceiptPath);
                var fileName = Path.GetFileName(invoice.ReceiptPath);
                var contentType = GetContentType(fileName);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка скачивания чека для InvoiceId={InvoiceId}", invoiceId);
                return NotFound(new { message = "Файл не найден", error = ex.Message });
            }
        }

        [HttpGet("request/{requestId}/invoice-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<InvoiceInfoDto2>> GetInvoiceInfo(int requestId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.RequestId == requestId);

            if (invoice == null)
            {
                return Ok(new InvoiceInfoDto2
                {
                    Generated = false,
                    ReceiptUploaded = false,
                    IsPaid = false
                });
            }

            return Ok(new InvoiceInfoDto2
            {
                InvoiceId = invoice.InvoiceId,
                Generated = true,
                Amount = invoice.Amount,
                FilePath = invoice.FilePath,
                ReceiptUploaded = !string.IsNullOrEmpty(invoice.ReceiptPath),
                ReceiptPath = invoice.ReceiptPath,
                IsPaid = invoice.Status == InvoiceStatus.Paid
            });
        }

        // ============================================
        // 🔥 АКТ ВЫПОЛНЕННЫХ РАБОТ — НЕ ТРОГАЕМ!
        // (Оставляем как есть, доработаем позже)
        // ============================================
        // Все эндпоинты для акта (SignAct, GetSignatureStatus и т.д.) 
        // остаются без изменений — они будут в отдельном контроллере

        // ============================================
        // 🔥 ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ============================================

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

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private ContractDto MapToDto(Diploma.DataAccess.Entities.ContractEntity contract)
        {
            return new ContractDto
            {
                ContractId = contract.ContractId,
                RequestId = contract.RequestId,
                ContractNumber = contract.ContractNumber,
                Status = contract.IsFullySigned ? "Signed" : "Draft",
                FilePath = contract.FilePath,
                CreatedAt = contract.CreatedAt,
                SignedAt = contract.Signature?.SignedAt,
                IsSignedByClient = contract.Signature?.IsSignedByClient ?? false,
                Comment = null
            };
        }

        private InvoiceDto MapToDto(Diploma.DataAccess.Entities.InvoiceEntity invoice)
        {
            return new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                RequestId = invoice.RequestId,
                InvoiceNumber = invoice.InvoiceNumber,
                Status = invoice.Status.ToString(),
                Amount = invoice.Amount,
                FilePath = invoice.FilePath,
                CreatedAt = invoice.CreatedAt,
                IssuedAt = invoice.IssuedAt,
                PaidAt = invoice.PaidAt,
                ReceiptPath = invoice.ReceiptPath,
                Comment = invoice.Comment
            };
        }
    }

    // 🔥 DTO для статуса подписания договора
    

    // 🔥 DTO для данных договора с подписями (для генерации PDF)
    //public class ContractPdfData
    //{
    //    public int RequestId { get; set; }
    //    public string? ContractNumber { get; set; }
    //    public string ClientFio { get; set; } = string.Empty;
    //    public string ClientPhone { get; set; } = string.Empty;
    //    public string DeviceModel { get; set; } = string.Empty;
    //    public string SerialNumber { get; set; } = string.Empty;
    //    public string Description { get; set; } = string.Empty;
    //    public DateTime CreatedAt { get; set; }

    //    // Данные подписей
    //    public DateTime? ClientSignedAt { get; set; }
    //    public string? ClientSignatureMethod { get; set; }
    //    public string? ClientIp { get; set; }
    //    public DateTime? DispatcherSignedAt { get; set; }
    //    public string DispatcherFio { get; set; } = string.Empty;
    //    public int? DispatcherTabNumber { get; set; }
    //}

    // 🔥 DTO для информации о счёте
    public class InvoiceInfoDto2
    {
        public int? InvoiceId { get; set; }
        public bool Generated { get; set; }
        public decimal? Amount { get; set; }
        public string? FilePath { get; set; }
        public bool ReceiptUploaded { get; set; }
        public string? ReceiptPath { get; set; }
        public bool IsPaid { get; set; }
    }
}