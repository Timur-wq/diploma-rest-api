using Diploma.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Счета")]
    public class InvoiceEntity
    {
        [Key]
        [Column("Код счёта")]
        public int InvoiceId { get; set; }

        [Column("Код заявки")]
        [Required]
        public int RequestId { get; set; }

        [Column("Номер счёта")]
        [StringLength(50)]
        public string? InvoiceNumber { get; set; }

        [Column("Статус счёта")]
        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        [Column("Сумма", TypeName = "decimal(10,2)")]
        [Required]
        public decimal Amount { get; set; }

        [Column("Путь к файлу")]
        [StringLength(500)]
        public string? FilePath { get; set; }

        [Column("Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Дата выставления")]
        public DateTime? IssuedAt { get; set; }

        [Column("Дата оплаты")]
        public DateTime? PaidAt { get; set; }

        [Column("Путь к чеку")]
        [StringLength(500)]
        public string? ReceiptPath { get; set; }

        [Column("Комментарий")]
        [StringLength(500)]
        public string? Comment { get; set; }

        // Навигационные свойства
        [ForeignKey(nameof(RequestId))]
        public virtual RequestEntity? Request { get; set; }

        [Column("Тип счёта")]
        [StringLength(50)]
        public string InvoiceType { get; set; } = "prepayment";  // "prepayment" или "final"
    }
}
