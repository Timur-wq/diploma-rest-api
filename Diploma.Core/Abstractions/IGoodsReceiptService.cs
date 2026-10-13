using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;

namespace Diploma.Core.Abstractions
{
    public interface IGoodsReceiptService
    {
        Task<GoodsReceiptDto> CreateReceiptAsync(CreateGoodsReceiptRequest request);
        Task<GoodsReceiptDto?> GetReceiptByIdAsync(int receiptId);
        Task<List<GoodsReceiptDto>> GetReceiptsByMtsEmployeeAsync(int employeeId);
        Task<List<GoodsReceiptDto>> GetPendingReceiptsAsync();
        Task<GoodsReceiptDto> AcceptGoodsAsync(int spareOrderId, AcceptGoodsRequest request);
        Task RejectOrderAsync(int spareOrderId, string comment);
    }
}
