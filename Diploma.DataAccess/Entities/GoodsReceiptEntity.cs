// Diploma.DataAccess/Entities/GoodsReceiptEntity.cs

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    /// <summary>
    /// Приходная накладная ЗИП
    /// </summary>
    [Table("Приходные накладные")]
    public class GoodsReceiptEntity
    {
        [Key]
        [Column("Код приходной накладной")]
        public int ReceiptId { get; set; }

        [Column("Код заявки на ЗИП")]
        [ForeignKey(nameof(SpareOrder))]
        public int SpareOrderId { get; set; }

        [Column("Заказано количество")]
        public int OrderedQuantity { get; set; }

        [Column("Принято количество")]
        public int ReceivedQuantity { get; set; }

        [Column("Годные количество")]
        public int GoodQuantity { get; set; }

        [Column("Брак количество")]
        public int DefectQuantity { get; set; }

        [Column("Комментарий")]
        public string? Comment { get; set; }

        [Column("Дата приёмки")]
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        [Column("Принял сотрудник")]
        public int? ReceivedByEmployeeId { get; set; }

        [Column("Статус приёмки")]
        public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Partial;

        // Навигация
        [ForeignKey(nameof(ReceivedByEmployeeId))]
        public virtual StafEntity? ReceivedByEmployee { get; set; }

        public virtual SpareOrderEntity? SpareOrder { get; set; }
    }

    /// <summary>
    /// Статус приёмки ЗИП
    /// </summary>
    public enum GoodsReceiptStatus
    {
        Full = 0,      // ✅ Всё принято без брака
        Partial = 1,   // ⚠️ Частичная приёмка (есть брак или недопоставка)
        Rejected = 2,  // ❌ Полный отказ от приёмки
        Returned = 3   // 🔄 Брак возвращён поставщику
    }
}