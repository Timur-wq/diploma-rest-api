using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diploma.Core.Enums;

namespace Diploma.DataAccess.Entities
{
    [Table("Договоры")]
    public class ContractEntity
    {
        [Key]
        [Column("Код договора")]
        public int ContractId { get; set; }

        [Column("Код заявки")]
        [Required]
        public int RequestId { get; set; }

        [Column("Номер договора")]
        [StringLength(50)]
        public string? ContractNumber { get; set; }

        //[Column("Статус договора")]
        //[Required]
        //public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Column("Путь к файлу")]
        [StringLength(500)]
        public string? FilePath { get; set; }

        [Column("Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        //[Column("Дата подписания")]
        //public DateTime? SignedAt { get; set; }

        //[Column("Подписан клиентом")]
        //public bool IsSignedByClient { get; set; } = false;

        //[Column("Комментарий")]
        //[StringLength(500)]
        //public string? Comment { get; set; }

        // Навигационные свойства
        [ForeignKey(nameof(RequestId))]
        public virtual RequestEntity? Request { get; set; }

        // 🔥 Навигация на подпись (одна запись на договор)
        [ForeignKey(nameof(SignatureId))]
        public virtual SignatureEntity? Signature { get; set; }

        
        public int? SignatureId { get; set; }

        // 🔥 Вычисляемое свойство для удобства
        [NotMapped]
        public bool IsFullySigned => Signature?.IsSignedByClient == true &&
                                      Signature?.IsSignedByDispatcher == true;
    }
}
