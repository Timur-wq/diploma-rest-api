using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    [Table("Перечень ЗИП - Акт диагностики")]
    [PrimaryKey(nameof(SpareCode), nameof(ActCode))]
    public class SparePartsDiagnosticAct
    {
        [Key]
        [Column("Код ЗИП")]
        [Required]
        [ForeignKey(nameof(Spare))]
        public int SpareCode { get; set; }

        [Key]
        [Column("Код акта")]
        [Required]
        [ForeignKey(nameof(DiagnosticAct))]
        public int ActCode { get; set; }

        [Column("Количество")]
        [Required]
        public int Amount { get; set; } = 1;

        [Column("Согласовано клиентом")]
        public bool? IsApprovedByClient { get; set; }

        // ✅ Навигационные свойства
        public virtual SpareEntity? Spare { get; set; }
        public virtual DiagnosticAct? DiagnosticAct { get; set; }
    }
}