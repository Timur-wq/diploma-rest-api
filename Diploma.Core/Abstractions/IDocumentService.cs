using Diploma.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;
using Diploma.Core.Models;

namespace Diploma.Core.Abstractions
{
    public interface IDocumentService
    {
        // === Договор ===

        /// <summary>
        /// Получить договор по заявке
        /// </summary>
        Task<ContractEntity?> GetContractByRequestIdAsync(int requestId);

        /// <summary>
        /// Сформировать договор (запись в БД)
        /// </summary>
        Task<ContractDto> GenerateContractAsync(int requestId, string filePath);

        /// <summary>
        /// Подтвердить подписание договора клиентом
        /// </summary>
        Task<bool> ConfirmContractSignedAsync(int contractId);

        // === Счёт ===

        /// <summary>
        /// Получить счёт по заявке
        /// </summary>
        Task<InvoiceEntity?> GetInvoiceByRequestIdAsync(int requestId);

        /// <summary>
        /// Сформировать счёт (запись в БД)
        /// </summary>
        Task<InvoiceDto> GenerateInvoiceAsync(int requestId, decimal amount, string filePath);

        /// <summary>
        /// Загрузить чек об оплате
        /// </summary>
        Task<bool> UploadPaymentReceiptAsync(int invoiceId, string receiptPath);

        /// <summary>
        /// Подтвердить оплату счёта
        /// </summary>
        Task<bool> ConfirmInvoicePaidAsync(int invoiceId);

        // 🔥 НОВЫЕ МЕТОДЫ:
        Task<bool> SignContractByClientAsync(int contractId, string method, string? ip, string? userAgent);
        Task<bool> SignContractByDispatcherAsync(int contractId, int dispatcherTabNum, string? ip, string? userAgent);
        Task<ContractSignatureStatusDto?> GetContractSignatureStatusAsync(int contractId);
    }
}
