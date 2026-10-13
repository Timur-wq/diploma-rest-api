// Diploma.API/Controllers/Mts/MtsGoodsReceiptController.cs

using Diploma.Core.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Diploma.Core.Class1;

namespace Diploma.API.Controllers.OmtEmployee
{
    [ApiController]
    [Route("api/OmtEmployee/[controller]")]
    [Authorize(Roles = "OmtEmployee")]  // Роль сотрудника ОМТ
    public class GoodsReceiptController : ControllerBase
    {
        private readonly IGoodsReceiptService _receiptService;
        private readonly ILogger<GoodsReceiptController> _logger;

        public GoodsReceiptController(
            IGoodsReceiptService receiptService,
            ILogger<GoodsReceiptController> logger)
        {
            _receiptService = receiptService;
            _logger = logger;
        }

        private int GetCurrentEmployeeId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }

        /// <summary>
        /// Получить заявки, ожидающие приёмки
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<List<GoodsReceiptDto>>> GetPendingReceipts()
        {
            var receipts = await _receiptService.GetPendingReceiptsAsync();
            return Ok(receipts);
        }

        /// <summary>
        /// Принять товар с указанием брака
        /// </summary>
        [HttpPost("{spareOrderId}/accept")]
        public async Task<ActionResult<GoodsReceiptDto>> AcceptGoods(
            int spareOrderId,
            [FromBody] AcceptGoodsRequest request)
        {
            try
            {
                request.EmployeeId = GetCurrentEmployeeId();
                var receipt = await _receiptService.AcceptGoodsAsync(spareOrderId, request);
                return Ok(receipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при приёмке заявки #{OrderId}", spareOrderId);
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Отклонить заказ полностью
        /// </summary>
        [HttpPost("{spareOrderId}/reject")]
        public async Task<IActionResult> RejectOrder(int spareOrderId, [FromBody] RejectOrderRequest request)
        {
            try
            {
                await _receiptService.RejectOrderAsync(spareOrderId, request.Comment);
                return Ok(new { message = "Заказ отклонён" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Получить историю приёмки сотрудника
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<List<GoodsReceiptDto>>> GetMyHistory()
        {
            var employeeId = GetCurrentEmployeeId();
            var receipts = await _receiptService.GetReceiptsByMtsEmployeeAsync(employeeId);
            return Ok(receipts);
        }

        /// <summary>
        /// Получить детальную информацию о приходной накладной
        /// </summary>
        [HttpGet("{receiptId}")]
        public async Task<ActionResult<GoodsReceiptDto>> GetReceipt(int receiptId)
        {
            var receipt = await _receiptService.GetReceiptByIdAsync(receiptId);
            if (receipt == null)
                return NotFound(new { message = "Приходная накладная не найдена" });

            return Ok(receipt);
        }
    }

    public class RejectOrderRequest
    {
        public string Comment { get; set; } = string.Empty;
    }
}