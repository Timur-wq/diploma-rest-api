using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Акт списания ЗИП")]
    public class SparePartsWriteOffEntity
    {
        [Key]
        [Column("Код акта списания ЗИП")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActCode { get; set; }

        // 🔥 Ссылка на конкретное предложение поставщика
        [Column("Код ЗИП у поставщика")]
        [Required]
        [ForeignKey(nameof(SupplierSpare))]
        public int SupplierSpareId { get; set; }

        [Column("Код наряда")]
        [Required]
        [ForeignKey(nameof(TaskEntity))]
        public int TaskEntityId { get; set; }

        // 🔥 Навигационные свойства
        public virtual SupplierSpareEntity? SupplierSpare { get; set; }

        public virtual TaskEntity? TaskEntity { get; set; }

        [Column("Списываемое количество")]
        [Required]
        public int Quantity { get; set; } = 1;

        // 🔥 НОВОЕ: Тип списания
        [Column("Тип списания")]
        public WriteOffType WriteOffType { get; set; } = WriteOffType.Installation;

        // 🔥 НОВОЕ: Связь с приходной накладной (для брака)
        [Column("Код приходной накладной")]
        public int? GoodsReceiptId { get; set; }

        [ForeignKey(nameof(GoodsReceiptId))]
        public virtual GoodsReceiptEntity? GoodsReceipt { get; set; }

        [Column("Дата списания")]
        public DateTime WriteOffDate { get; set; } = DateTime.UtcNow;

        [Column("Комментарий")]
        public string? Comment { get; set; }

        [Column("Путь к файлу")]
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Тип списания ЗИП
    /// </summary>
    public enum WriteOffType
    {
        Installation = 0,    // 🔧 Установка в заявку клиента
        Defect = 1,          // ❌ Брак при приёмке
        Return = 2,          // 🔄 Возврат поставщику
        WriteOff = 3         // 🗑️ Списание (истёк срок и т.п.)
    }
}
