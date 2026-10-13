using Diploma.DataAccess.Entities;
using Diploma.DataAccess;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Diploma.Core.Class1;
using Microsoft.EntityFrameworkCore;
using Diploma.Core.Abstractions;

namespace Diploma.Application.Services
{
    public class GoodsReceiptService : IGoodsReceiptService
    {
        private readonly DiplomaDbContext _context;
        private readonly ILogger<GoodsReceiptService> _logger;

        public GoodsReceiptService(
            DiplomaDbContext context,
            ILogger<GoodsReceiptService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Создать приходную накладную (ручное создание)
        /// </summary>
        public async Task<GoodsReceiptDto> CreateReceiptAsync(CreateGoodsReceiptRequest request)
        {
            var order = await _context.SpareOrders
                .Include(o => o.SupplierSpare)
                    .ThenInclude(ss => ss.Spare)
                .Include(o => o.SupplierSpare)
                    .ThenInclude(ss => ss.Supplier)
                .FirstOrDefaultAsync(o => o.OrderId == request.SpareOrderId);

            if (order == null)
                throw new Exception("Заявка на ЗИП не найдена");

            if (order.Status != SpareOrderStatus.Confirmed)
                throw new Exception("Можно принимать только подтверждённые заказы");

            var receipt = new GoodsReceiptEntity
            {
                SpareOrderId = request.SpareOrderId,
                OrderedQuantity = order.Quantity,
                ReceivedQuantity = request.ReceivedQuantity,
                GoodQuantity = request.GoodQuantity,
                DefectQuantity = request.DefectQuantity,
                Comment = request.Comment,
                ReceivedAt = DateTime.UtcNow,
                ReceivedByEmployeeId = request.ReceivedByEmployeeId,
                Status = DetermineReceiptStatus(request)
            };

            _context.GoodsReceipts.Add(receipt);
            await _context.SaveChangesAsync();

            // 🔥 Обновляем заявку
            order.GoodsReceiptId = receipt.ReceiptId;
            order.ActuallyReceived = request.ReceivedQuantity;
            order.Status = receipt.Status == GoodsReceiptStatus.Full
                ? SpareOrderStatus.Delivered
                : SpareOrderStatus.PartialDelivered;

            // 🔥 Обновляем остатки (ТОЛЬКО годные!)
            if (request.GoodQuantity > 0)
            {
                var spare = await _context.Spares
                    .FirstOrDefaultAsync(s => s.SpareCode == order.SupplierSpare.SpareCode);

                if (spare != null)
                {
                    spare.Amount += request.GoodQuantity;
                }
            }

            // 🔥 Если есть брак — создаём акт списания
            if (request.DefectQuantity > 0)
            {
                _context.SparePartsWriteOffEntities.Add(new SparePartsWriteOffEntity
                {
                    SupplierSpareId = order.SupplierSpareId,
                    Quantity = request.DefectQuantity,
                    WriteOffType = WriteOffType.Defect,
                    GoodsReceiptId = receipt.ReceiptId,
                    Comment = $"Брак при приёмке. Заявка #{order.OrderId}",
                    WriteOffDate = DateTime.UtcNow
                });

                // 🔥 Создаём заявку на замену брака
                _context.SpareOrders.Add(new SpareOrderEntity
                {
                    SupplierSpareId = order.SupplierSpareId,
                    Quantity = request.DefectQuantity,
                    Status = SpareOrderStatus.Reservation,
                    CreatedAt = DateTime.UtcNow,
                    ActCode = order.ActCode
                });

                _logger.LogInformation("🔄 Создана заявка на замену брака: ЗИП #{SpareCode} × {Qty}",
                    order.SupplierSpare.SpareCode, request.DefectQuantity);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Приходная накладная #{ReceiptId} создана. Годные: {GoodQty}, Брак: {DefectQty}",
                receipt.ReceiptId, request.GoodQuantity, request.DefectQuantity);

            return MapToDto(receipt, order);
        }

        /// <summary>
        /// Принять товар (упрощённый метод для МТС)
        /// </summary>
        public async Task<GoodsReceiptDto> AcceptGoodsAsync(int spareOrderId, AcceptGoodsRequest request)
        {
            var order = await _context.SpareOrders
                .Include(o => o.SupplierSpare)
                    .ThenInclude(ss => ss.Spare)
                .FirstOrDefaultAsync(o => o.OrderId == spareOrderId);

            if (order == null)
                throw new Exception("Заявка не найдена");

            var receiptRequest = new CreateGoodsReceiptRequest
            {
                SpareOrderId = spareOrderId,
                ReceivedQuantity = request.ReceivedQuantity,
                GoodQuantity = request.GoodQuantity,
                DefectQuantity = request.ReceivedQuantity - request.GoodQuantity,
                Comment = request.Comment,
                ReceivedByEmployeeId = request.EmployeeId,
                Status = request.DefectQuantity > 0
                    ? GoodsReceiptStatus.Partial
                    : GoodsReceiptStatus.Full
            };

            return await CreateReceiptAsync(receiptRequest);
        }

        /// <summary>
        /// Отклонить заказ полностью
        /// </summary>
        public async Task RejectOrderAsync(int spareOrderId, string comment)
        {
            var order = await _context.SpareOrders.FindAsync(spareOrderId);
            if (order == null)
                throw new Exception("Заявка не найдена");

            order.Status = SpareOrderStatus.Cancelled;

            var receipt = new GoodsReceiptEntity
            {
                SpareOrderId = spareOrderId,
                OrderedQuantity = order.Quantity,
                ReceivedQuantity = 0,
                GoodQuantity = 0,
                DefectQuantity = 0,
                Comment = comment,
                Status = GoodsReceiptStatus.Rejected,
                ReceivedAt = DateTime.UtcNow
            };

            _context.GoodsReceipts.Add(receipt);
            await _context.SaveChangesAsync();

            _logger.LogInformation("❌ Заявка #{OrderId} отклонена: {Comment}", spareOrderId, comment);
        }

        public async Task<GoodsReceiptDto?> GetReceiptByIdAsync(int receiptId)
        {
            var receipt = await _context.GoodsReceipts
                .Include(r => r.SpareOrder)
                    .ThenInclude(o => o.SupplierSpare)
                        .ThenInclude(ss => ss.Spare)
                .Include(r => r.SpareOrder)
                    .ThenInclude(o => o.SupplierSpare)
                        .ThenInclude(ss => ss.Supplier)
                .Include(r => r.ReceivedByEmployee)
                .FirstOrDefaultAsync(r => r.ReceiptId == receiptId);

            return receipt == null ? null : MapToDto(receipt, receipt.SpareOrder);
        }

        public async Task<List<GoodsReceiptDto>> GetPendingReceiptsAsync()
        {
            var orders = await _context.SpareOrders
                .Include(o => o.SupplierSpare)
                    .ThenInclude(ss => ss.Spare)
                .Include(o => o.SupplierSpare)
                    .ThenInclude(ss => ss.Supplier)
                .Where(o => o.Status == SpareOrderStatus.Confirmed && o.GoodsReceiptId == null)
                .ToListAsync();

            return orders.Select(o => new GoodsReceiptDto
            {
                SpareOrderId = o.OrderId,
                OrderedQuantity = o.Quantity,
                SpareName = o.SupplierSpare?.Spare?.SpareName ?? $"ЗИП #{o.SupplierSpareId}",
                SupplierName = o.SupplierSpare?.Supplier?.Name ?? $"Поставщик #{o.SupplierSpare?.SupplierId}"
            }).ToList();
        }

        public async Task<List<GoodsReceiptDto>> GetReceiptsByMtsEmployeeAsync(int employeeId)
        {
            var receipts = await _context.GoodsReceipts
                .Include(r => r.SpareOrder)
                    .ThenInclude(o => o.SupplierSpare)
                        .ThenInclude(ss => ss.Spare)
                .Include(r => r.ReceivedByEmployee)
                .Where(r => r.ReceivedByEmployeeId == employeeId)
                .OrderByDescending(r => r.ReceivedAt)
                .ToListAsync();

            return receipts.Select(r => MapToDto(r, r.SpareOrder)).ToList();
        }

        private static GoodsReceiptStatus DetermineReceiptStatus(CreateGoodsReceiptRequest request)
        {
            if (request.DefectQuantity == 0 && request.ReceivedQuantity >= request.GoodQuantity)
                return GoodsReceiptStatus.Full;

            if (request.ReceivedQuantity == 0)
                return GoodsReceiptStatus.Rejected;

            return GoodsReceiptStatus.Partial;
        }

        private GoodsReceiptDto MapToDto(GoodsReceiptEntity receipt, SpareOrderEntity? order)
        {
            return new GoodsReceiptDto
            {
                ReceiptId = receipt.ReceiptId,
                SpareOrderId = receipt.SpareOrderId,
                OrderedQuantity = receipt.OrderedQuantity,
                ReceivedQuantity = receipt.ReceivedQuantity,
                GoodQuantity = receipt.GoodQuantity,
                DefectQuantity = receipt.DefectQuantity,
                Comment = receipt.Comment,
                ReceivedAt = receipt.ReceivedAt,
                ReceivedByEmployeeId = receipt.ReceivedByEmployeeId,
                ReceivedByEmployeeFio = receipt.ReceivedByEmployee?.User?.FullName,
                Status = receipt.Status,
                SpareName = order?.SupplierSpare?.Spare?.SpareName ?? "Неизвестно",
                SupplierName = order?.SupplierSpare?.Supplier?.Name ?? "Неизвестно"
            };
        }
    }

    
}
