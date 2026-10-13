using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    [Table("Перечень работ - Акт диагностики")]
    [PrimaryKey(nameof(WorkCode), nameof(ActCode))]
    public class WorksDiagnosticAct  // ✅ Исправлено: было WorksDiagnosticAtc
    {
        [Key]
        [Column("Код работы")]
        [Required]
        [ForeignKey(nameof(Work))]
        public int WorkCode { get; set; }

        [Key]
        [Column("Код акта")]
        [Required]
        [ForeignKey(nameof(DiagnosticAct))]
        public int ActCode { get; set; }

        [Column("Согласовано клиентом")]
        public bool? IsApprovedByClient { get; set; }

        // ✅ Навигационные свойства
        public virtual WorkEntity? Work { get; set; }
        public virtual DiagnosticAct? DiagnosticAct { get; set; }
    }
}