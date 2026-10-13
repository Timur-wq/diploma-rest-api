using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Diploma.API.Controllers
{
    [ApiController]
    [Route("api/client/Document")]
    [Authorize(Roles = "Client")]
    public class ClientDocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IPdfGenerationService _pdfService;
        private readonly IFileStorageService _fileStorage;
        private readonly DiplomaDbContext _context;
        private readonly ILogger<ClientDocumentController> _logger;  // ✅ Исправлено: ClientDocumentController
        private readonly IWebHostEnvironment _env;

        public ClientDocumentController(
            IDocumentService documentService,
            IPdfGenerationService pdfService,
            IFileStorageService fileStorage,
            DiplomaDbContext context,
            IWebHostEnvironment env,
            ILogger<ClientDocumentController> logger)  // ✅ Исправлено: ClientDocumentController
        {
            _documentService = documentService;
            _pdfService = pdfService;
            _fileStorage = fileStorage;
            _context = context;
            _env = env;
            _logger = logger;
        }

        private int GetCurrentClientId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        /// <summary>
        /// Получить информацию о счёте для заявки
        /// </summary>
        [HttpGet("request/{requestId}/invoice-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<InvoiceInfoDto>> GetInvoiceInfo(int requestId)
        {
            var clientId = GetCurrentClientId();

            var request = await _context.Requests
                .Include(r => r.Contract)
                .Include(r => r.Invoices)  // 🔥 ИСПРАВЛЕНО: включаем коллекцию
                .FirstOrDefaultAsync(r => r.Id == requestId && r.ClientId == clientId);

            if (request == null)
                return NotFound(new { message = "Заявка не найдена" });

            // 🔥 Получаем счёт на предоплату из коллекции
            var prepaymentInvoice = request.Invoices?
                .FirstOrDefault(i => i.InvoiceType == "prepayment" || i.InvoiceType == null);

            return Ok(new InvoiceInfoDto
            {
                invoiceId = prepaymentInvoice.InvoiceId,
                ContractSigned = request.Contract?.Signature?.IsSignedByClient == true,
                InvoiceGenerated = prepaymentInvoice != null,
                InvoiceFilePath = prepaymentInvoice?.FilePath,
                InvoiceAmount = prepaymentInvoice?.Amount,
                PaymentReceiptUploaded = !string.IsNullOrEmpty(prepaymentInvoice?.ReceiptPath),
                PaymentReceiptPath = prepaymentInvoice?.ReceiptPath,
                IsPrepaymentConfirmed = prepaymentInvoice?.Status == InvoiceStatus.Paid
            });
        }

        /// <summary>
        /// Сформировать счёт на предоплату (только для диспетчера)
        /// </summary>
        [HttpPost("request/{requestId}/generate-invoice")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GenerateInvoice(int requestId)
        {
            try
            {
                var contract = await _context.Contracts
    .Include(c => c.Signature)
    .FirstOrDefaultAsync(c => c.RequestId == requestId &&
                              c.Signature != null &&
                              c.Signature.IsSignedByClient);

                if (contract == null)
                    return BadRequest(new { message = "Договор должен быть подписан" });

                // 🔥 Проверяем, нет ли уже счёта на предоплату
                var existingInvoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.RequestId == requestId &&
                                             (i.InvoiceType == "prepayment" || i.InvoiceType == null));

                if (existingInvoice != null)
                    return Ok(new
                    {
                        message = "Счёт уже сформирован",
                        invoice = new
                        {
                            invoiceId = existingInvoice.InvoiceId,
                            filePath = existingInvoice.FilePath,
                            amount = existingInvoice.Amount
                        }
                    });

                var act = await _context.DiagnosticActs
                    .Include(a => a.SpareParts)
                    .FirstOrDefaultAsync(a => a.Task!.RequestId == requestId);

                var sparePartsCost = act?.SpareParts
                    .Where(s => s.IsApprovedByClient == true)
                    .Sum(s => {
                        var supplierSpare = _context.SupplierSpares
                            .FirstOrDefault(ss => ss.SpareCode == s.SpareCode);
                        return (supplierSpare?.UnitPrice ?? 0) * s.Amount;
                    }) ?? 0;

                var fileName = $"Invoice_Prepayment_{requestId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var relativePath = Path.Combine("documents", "invoices", fileName);
                var fullPath = Path.Combine(_env.WebRootPath, relativePath);

                // Создаём директорию если нет
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                await _pdfService.GeneratePrepaymentInvoiceAsync(requestId, fullPath);

                // 🔥 Создаём новый счёт с InvoiceType = "prepayment"
                var invoice = new InvoiceEntity
                {
                    RequestId = requestId,
                    Amount = sparePartsCost,
                    FilePath = relativePath,
                    Status = InvoiceStatus.Draft,
                    CreatedAt = DateTime.UtcNow,
                    InvoiceNumber = $"PRE-{requestId}-{DateTime.UtcNow:yyyyMMdd}",
                    InvoiceType = "prepayment",  // 🔥 Указываем тип счёта
                    Comment = "Счёт на предоплату ЗИП"
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

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

        /// <summary>
        /// Скачать счёт
        /// </summary>
        [HttpGet("invoice/{invoiceId}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadInvoice(int invoiceId)
        {
            var clientId = GetCurrentClientId();

            var invoice = await _context.Invoices
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.Request.ClientId == clientId);

            if (invoice == null || string.IsNullOrEmpty(invoice.FilePath))
                return NotFound(new { message = "Счёт не найден" });

            try
            {
                var fileBytes = await _fileStorage.GetFileAsync(invoice.FilePath);
                var fileName = Path.GetFileName(invoice.FilePath);

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = "Файл не найден", error = ex.Message });
            }
        }

        /// <summary>
        /// Загрузить чек об оплате
        /// </summary>
        [HttpPost("invoice/{invoiceId}/upload-receipt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadReceipt(int invoiceId, IFormFile receipt)
        {
            var clientId = GetCurrentClientId();

            var invoice = await _context.Invoices
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId && i.Request.ClientId == clientId);

            if (invoice == null)
                return NotFound(new { message = "Счёт не найден" });

            if (receipt == null || receipt.Length == 0)
                return BadRequest(new { message = "Файл не предоставлен" });

            if (!receipt.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) &&
                !receipt.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !receipt.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Допустимые форматы: PDF, JPG, PNG" });

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

        // Diploma.API/Controllers/Client/ClientDocumentController.cs

        [HttpGet("request/{requestId}/warranty-info")]
        public async Task<ActionResult<WarrantyInfoResponse>> GetWarrantyInfo(int requestId)
        {
            var clientId = GetCurrentClientId(); // Ваша логика получения ID клиента

            var warranty = await _context.WarrantyCards
                .Include(w => w.Request)
                .FirstOrDefaultAsync(w =>
                    w.RequestId == requestId &&
                    w.Request.ClientId == clientId &&
                    w.IsVisibleToClient &&
                    w.IsActive);

            if (warranty == null)
                return Ok(new WarrantyInfoResponse { WarrantyExists = false });

            return Ok(new WarrantyInfoResponse
            {
                WarrantyExists = true,
                WarrantyFilePath = warranty.FilePath,
                WarrantyValidUntil = warranty.ValidUntil
            });
        }

        public class WarrantyInfoResponse
        {
            public bool WarrantyExists { get; set; }
            public string? WarrantyFilePath { get; set; }
            public DateTime? WarrantyValidUntil { get; set; }
        }

        [HttpGet("warranty/download")]
        public async Task<IActionResult> DownloadWarranty([FromQuery] string path)
        {
            if (string.IsNullOrEmpty(path))
                return BadRequest(new { message = "Путь к файлу не указан" });

            var clientId = GetCurrentClientId();

            // Проверяем, что файл принадлежит этому клиенту
            var warranty = await _context.WarrantyCards
                .Include(w => w.Request)
                .FirstOrDefaultAsync(w =>
                    w.FilePath == path &&
                    w.Request.ClientId == clientId &&
                    w.IsVisibleToClient);

            if (warranty == null)
                return NotFound(new { message = "Файл не найден" });

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.TrimStart('/'));

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = "Файл не найден на сервере" });

            return PhysicalFile(fullPath, "application/pdf", $"Warranty_{warranty.RequestId}.pdf");
        }

        /// <summary>
        /// Получить информацию о финальном счёте на оплату работ
        /// </summary>
        [HttpGet("request/{requestId}/final-invoice-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<FinalInvoiceInfoDto>> GetFinalInvoiceInfo(int requestId)
        {
            var clientId = GetCurrentClientId();

            var request = await _context.Requests
                .Include(r => r.Invoices)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.ClientId == clientId);

            if (request == null)
                return NotFound(new { message = "Заявка не найдена" });

            // 🔥 Получаем финальный счёт на оплату работ
            var finalInvoice = request.Invoices?
                .FirstOrDefault(i => i.InvoiceType == "final");

            return Ok(new FinalInvoiceInfoDto
            {
                InvoiceGenerated = finalInvoice != null,
                InvoiceFilePath = finalInvoice?.FilePath,
                InvoiceAmount = finalInvoice?.Amount,
                PaymentReceiptUploaded = !string.IsNullOrEmpty(finalInvoice?.ReceiptPath),
                PaymentReceiptPath = finalInvoice?.ReceiptPath,
                IsPaymentConfirmed = finalInvoice?.Status == InvoiceStatus.Paid
            });
        }

        /// <summary>
        /// Скачать финальный счёт на оплату работ
        /// </summary>
        [HttpGet("request/{requestId}/final-invoice/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadFinalInvoice(int requestId)
        {
            var clientId = GetCurrentClientId();

            var invoice = await _context.Invoices
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.RequestId == requestId &&
                                          i.Request.ClientId == clientId &&
                                          i.InvoiceType == "final");

            if (invoice == null || string.IsNullOrEmpty(invoice.FilePath))
                return NotFound(new { message = "Счёт на оплату работ не найден" });

            try
            {
                var fileBytes = await _fileStorage.GetFileAsync(invoice.FilePath);
                var fileName = Path.GetFileName(invoice.FilePath);

                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = "Файл не найден", error = ex.Message });
            }
        }

        /// <summary>
        /// Загрузить чек об оплате финального счёта
        /// </summary>
        [HttpPost("request/{requestId}/final-invoice/upload-receipt")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadFinalInvoiceReceipt(int requestId, IFormFile receipt)
        {
            var clientId = GetCurrentClientId();

            var invoice = await _context.Invoices
                .Include(i => i.Request)
                .FirstOrDefaultAsync(i => i.RequestId == requestId &&
                                          i.Request.ClientId == clientId &&
                                          i.InvoiceType == "final");

            if (invoice == null)
                return NotFound(new { message = "Счёт на оплату работ не найден" });

            if (receipt == null || receipt.Length == 0)
                return BadRequest(new { message = "Файл не предоставлен" });

            if (!receipt.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) &&
                !receipt.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !receipt.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Допустимые форматы: PDF, JPG, PNG" });

            try
            {
                var receiptPath = await _fileStorage.SaveFileAsync(receipt, "receipts");

                // Обновляем счёт
                invoice.ReceiptPath = receiptPath;
                invoice.Status = InvoiceStatus.Pending; // Ожидает подтверждения
                await _context.SaveChangesAsync();

                return Ok(new { message = "Чек загружен", receiptPath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DTO для финального счёта
        public class FinalInvoiceInfoDto
        {
            public bool InvoiceGenerated { get; set; }
            public string? InvoiceFilePath { get; set; }
            public decimal? InvoiceAmount { get; set; }
            public bool PaymentReceiptUploaded { get; set; }
            public string? PaymentReceiptPath { get; set; }
            public bool IsPaymentConfirmed { get; set; }
        }
    }



    // ✅ DTO для ответа - добавлен класс
    public class InvoiceInfoDto
    {
        public decimal invoiceId { get; set; }
        public bool ContractSigned { get; set; }
        public bool InvoiceGenerated { get; set; }
        public string? InvoiceFilePath { get; set; }
        public decimal? InvoiceAmount { get; set; }
        public bool PaymentReceiptUploaded { get; set; }
        public string? PaymentReceiptPath { get; set; }
        public bool IsPrepaymentConfirmed { get; set; }
    }
}