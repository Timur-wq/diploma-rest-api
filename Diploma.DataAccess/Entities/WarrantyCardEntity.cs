using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Гарантийные талоны")]
    public class WarrantyCardEntity
    {
        [Key]
        [Column("Код талона")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Код заявки")]
        [Required]
        public int RequestId { get; set; }

        [Column("Код акта")]
        [Required]
        public int ActId { get; set; }

        [Column("Путь к файлу PDF")]
        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Column("Дата выдачи")]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [Column("Гарантийный срок (мес)")]
        public int WarrantyMonths { get; set; } = 3;

        [Column("Действует до")]
        public DateTime ValidUntil { get; set; }

        [Column("Активен")]
        public bool IsActive { get; set; } = true;

        [Column("Видим клиенту")]
        public bool IsVisibleToClient { get; set; } = false;  // 🔥 Новое поле

        // 🔥 Навигация
        [ForeignKey(nameof(RequestId))]
        public virtual RequestEntity? Request { get; set; }

        [ForeignKey(nameof(ActId))]
        public virtual RepairActEntity? Act { get; set; }
    }
}
