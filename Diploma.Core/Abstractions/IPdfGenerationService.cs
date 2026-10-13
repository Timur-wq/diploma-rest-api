using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface IPdfGenerationService
    {
        Task<string> GenerateContractAsync(int requestId, string outputPath);
        Task<string> GenerateInvoiceAsync(int requestId, decimal amount, string outputPath);
        Task<string> GeneratePrepaymentInvoiceAsync(int requestId, string outputPath);

        // 🔥 НОВЫЕ МЕТОДЫ:
        Task<string> GenerateRepairActAsync(RepairActData data);
        Task<string> GenerateSpareWriteOffAsync(SpareWriteOffData data);

        Task<string> GenerateWarrantyCardAsync(WarrantyData data);

        Task<string> GenerateInvoiceAsync2(int requestId, decimal amount, string outputPath);

        Task<string> GenerateContractWithSignaturesAsync(ContractPdfData data);

        Task GenerateAccountingReportAsync(AccountingReportData data, string outputPath);
    }
}
