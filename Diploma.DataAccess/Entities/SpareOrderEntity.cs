// Diploma.DataAccess/Entities/SpareOrderEntity.cs

using Diploma.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    [Table("Заявки на ЗИП")]
    public class SpareOrderEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ← Автоинкремент!
        [Column("Код заявки на ЗИП")]
        public int OrderId { get; set; }

        [Column("Закупаемое количество")]
        [Required]
        public int Quantity { get; set; } = 1;

        // 🔥 Ссылка на конкретное предложение поставщика
        [Column("Код ЗИП у поставщика")]
        [Required]
        [ForeignKey(nameof(SupplierSpare))]
        public int SupplierSpareId { get; set; }

        [Column("Статус")]
        [Required]
        public SpareOrderStatus Status { get; set; } = SpareOrderStatus.Reservation;

        [Column("Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 🔥 Навигационные свойства
        public virtual SupplierSpareEntity? SupplierSpare { get; set; }

        // Доступ к ЗИП и поставщику через SupplierSpare
        public SpareEntity? Spare => SupplierSpare?.Spare;
        public SupplierEntity? Supplier => SupplierSpare?.Supplier;

        // 🔥 НОВОЕ ПОЛЕ: Связь с актом
        [Column("Код акта")]
        public int? ActCode { get; set; }  // nullable, т.к. старые записи могут быть без акта

        [ForeignKey(nameof(ActCode))]
        public virtual DiagnosticAct? Act { get; set; }

        // 🔥 НОВОЕ: Связь с приходной накладной
        [Column("Код приходной накладной")]
        public int? GoodsReceiptId { get; set; }

        // 🔥 НОВОЕ: Фактически получено (может отличаться от заказанного)
        [Column("Фактически получено")]
        public int? ActuallyReceived { get; set; }

        [ForeignKey(nameof(GoodsReceiptId))]
        public virtual GoodsReceiptEntity? GoodsReceipt { get; set; }
    }

    public enum SpareOrderStatus
    {
        Reservation = 0,    // 🔹 Бронь (создано алгоритмом)
        Confirmed = 1,      // 🔹 Подтверждено (МТС подтвердил)
        Delivered = 2,      // 🔹 Доставлено (поступило на склад)
        Cancelled = 3,      // 🔹 Отменено
        PartialDelivered = 4 // 🔹 Частично доставлено (есть брак)
    }
}