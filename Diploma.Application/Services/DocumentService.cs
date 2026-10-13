// Diploma.Application/Services/DocumentService.cs
using Diploma.Core.Abstractions;
using Diploma.Core.Enums;
using Diploma.DataAccess;
using Diploma.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Application.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(DiplomaDbContext context, ILogger<DocumentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // === Методы для договора ===

        public async Task<ContractEntity?> GetContractByRequestIdAsync(int requestId)
        {
            return await _context.Contracts
                .Include(c => c.Signature)  // 🔥 Загружаем подпись
                .FirstOrDefaultAsync(c => c.RequestId == requestId);
        }

        public async Task<ContractDto> GenerateContractAsync(int requestId, string filePath)
        {
            var request = await _context.Requests.FindAsync(requestId);
            if (request == null)
                throw new Exception("Заявка не найдена");

            // Проверяем, что акт согласован
            var act = await _context.DiagnosticActs
                .Include(a => a.Task)
                .FirstOrDefaultAsync(a => a.Task != null && a.Task.RequestId == requestId);

            if (act?.Status != DiagnosticActStatus.ApprovedByClient)
                throw new Exception("Акт не согласован клиентом");

            // Проверяем, нет ли уже договора
            var existing = await GetContractByRequestIdAsync(requestId);
            if (existing != null)
            {
                existing.FilePath = filePath;
                existing.CreatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return MapToDto(existing);
            }

            // Создаём новый договор (без статуса и полей подписи)
            var contract = new ContractEntity
            {
                RequestId = requestId,
                ContractNumber = $"CONTRACT-{requestId}-{DateTime.Now:yyyyMMdd}",
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow
            };

            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();

            // 🔥 Создаём запись подписи для этого договора
            var signature = new SignatureEntity
            {
                Contract = contract
            };
            _context.Signatures.Add(signature);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Договор сформирован для заявки #{RequestId}", requestId);
            return MapToDto(contract);
        }

        // 🔥 НОВЫЙ МЕТОД: Подписать договор через SignatureEntity
        public async Task<bool> SignContractByClientAsync(int contractId, string method, string? ip, string? userAgent)
        {
            var contract = await _context.Contracts
                .Include(c => c.Signature)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null) return false;

            // Если подписи нет — создаём
            if (contract.Signature == null)
            {
                contract.Signature = new SignatureEntity { Contract = contract };
                _context.Signatures.Add(contract.Signature);
            }

            // Подписываем через SignatureEntity
            contract.Signature.IsSignedByClient = true;
            contract.Signature.SignedAt = DateTime.UtcNow;
            contract.Signature.SignatureMethod = method;
            contract.Signature.ClientVerificationData = JsonSerializer.Serialize(new
            {
                ip,
                userAgent,
                signed_at = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Клиент подписал договор #{ContractId}", contractId);
            return true;
        }

        // 🔥 НОВЫЙ МЕТОД: Подписать договор диспетчером
        public async Task<bool> SignContractByDispatcherAsync(int contractId, int dispatcherTabNum, string? ip, string? userAgent)
        {
            var contract = await _context.Contracts
                .Include(c => c.Signature)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null) return false;

            // Если подписи нет — создаём
            if (contract.Signature == null)
            {
                contract.Signature = new SignatureEntity { Contract = contract };
                _context.Signatures.Add(contract.Signature);
            }

            // Проверяем, что клиент уже подписал
            if (!contract.Signature.IsSignedByClient)
                return false;

            // Подписываем диспетчером
            contract.Signature.IsSignedByDispatcher = true;
            contract.Signature.DispatcherSignedAt = DateTime.UtcNow;
            contract.Signature.DispatcherSignedByTabNumber = dispatcherTabNum;
            contract.Signature.DispatcherVerificationData = JsonSerializer.Serialize(new
            {
                dispatcher_tab = dispatcherTabNum,
                ip,
                userAgent,
                signed_at = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Диспетчер #{TabNum} подписал договор #{ContractId}", dispatcherTabNum, contractId);
            return true;
        }

        // 🔥 НОВЫЙ МЕТОД: Получить статус подписания
        public async Task<ContractSignatureStatusDto?> GetContractSignatureStatusAsync(int contractId)
        {
            var contract = await _context.Contracts
                .Include(c => c.Signature)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null) return null;

            var signature = contract.Signature;

            return new ContractSignatureStatusDto
            {
                ContractId = contractId,
                IsSignedByClient = signature?.IsSignedByClient ?? false,
                ClientSignedAt = signature?.SignedAt,
                ClientSignatureMethod = signature?.SignatureMethod,
                IsSignedByDispatcher = signature?.IsSignedByDispatcher ?? false,
                DispatcherSignedAt = signature?.DispatcherSignedAt,
                DispatcherSignedByTabNumber = signature?.DispatcherSignedByTabNumber,
                IsFullySigned = contract.IsFullySigned
            };
        }

        // === Методы для счёта ===

        public async Task<InvoiceEntity?> GetInvoiceByRequestIdAsync(int requestId)
        {
            return await _context.Invoices
                .FirstOrDefaultAsync(i => i.RequestId == requestId);
        }

        public async Task<InvoiceDto> GenerateInvoiceAsync(int requestId, decimal amount, string filePath)
        {
            var request = await _context.Requests.FindAsync(requestId);
            if (request == null)
                throw new Exception("Заявка не найдена");

            // 🔥 Проверяем, что договор подписан (через SignatureEntity)
            var contract = await _context.Contracts
                .Include(c => c.Signature)
                .FirstOrDefaultAsync(c => c.RequestId == requestId);

            if (contract == null || !contract.IsFullySigned)
                throw new Exception("Договор должен быть полностью подписан");

            // Проверяем, нет ли уже счёта
            var existing = await GetInvoiceByRequestIdAsync(requestId);
            if (existing != null)
            {
                existing.FilePath = filePath;
                existing.Amount = amount;
                existing.Status = InvoiceStatus.Issued;
                existing.IssuedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return MapToDto(existing);
            }

            // Создаём новый счёт
            var invoice = new InvoiceEntity
            {
                RequestId = requestId,
                InvoiceNumber = $"INV-PREPAY-{requestId}-{DateTime.Now:yyyyMMdd}",
                Status = InvoiceStatus.Issued,
                Amount = amount,
                FilePath = filePath,
                CreatedAt = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Счёт на предоплату сформирован для заявки #{RequestId}", requestId);
            return MapToDto(invoice);
        }

        public async Task<bool> UploadPaymentReceiptAsync(int invoiceId, string receiptPath)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            invoice.ReceiptPath = receiptPath;
            invoice.IssuedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Чек загружен для счёта #{invoiceId}", invoiceId);
            return true;
        }

        public async Task<bool> ConfirmPrepaymentAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Предоплата подтверждена для счёта #{InvoiceId}", invoiceId);
            return true;
        }

        public async Task<bool> ConfirmInvoicePaidAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Оплата подтверждена для счёта #{InvoiceId}", invoiceId);
            return true;
        }

        // === Вспомогательные методы маппинга ===

        private ContractDto MapToDto(ContractEntity contract)
        {
            return new ContractDto
            {
                ContractId = contract.ContractId,
                RequestId = contract.RequestId,
                ContractNumber = contract.ContractNumber,
                Status = contract.IsFullySigned ? "Signed" : "Draft",  // 🔥 Вычисляем статус
                FilePath = contract.FilePath,
                CreatedAt = contract.CreatedAt,
                SignedAt = contract.Signature?.SignedAt,  // 🔥 Берём из подписи
                IsSignedByClient = contract.Signature?.IsSignedByClient ?? false,  // 🔥 Из подписи
                Comment = null  // 🔥 Убрано
            };
        }

        private InvoiceDto MapToDto(InvoiceEntity invoice)
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

        // Diploma.Application/Services/DocumentService.cs

        public async Task<bool> ConfirmContractSignedAsync(int contractId)
        {
            // 🔥 Это старый метод — для обратной совместимости
            // Перенаправляем вызов на новый метод подписания клиентом

            var contract = await _context.Contracts
                .Include(c => c.Signature)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null) return false;

            // Если подписи нет — создаём
            if (contract.Signature == null)
            {
                contract.Signature = new SignatureEntity { Contract = contract };
                _context.Signatures.Add(contract.Signature);
            }

            // Подписываем через SignatureEntity
            contract.Signature.IsSignedByClient = true;
            contract.Signature.SignedAt = DateTime.UtcNow;
            contract.Signature.SignatureMethod = "legacy";  // Метка для старых вызовов

            await _context.SaveChangesAsync();
            _logger.LogInformation("✅ Договор #{ContractId} подписан (через старый метод)", contractId);
            return true;
        }

        // Diploma.Application/Services/DocumentService.cs

        // Diploma.Application/Services/DocumentService.cs

    }

    
}