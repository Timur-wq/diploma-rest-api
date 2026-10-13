// Diploma.DataAccess/Entities/WorkSpareRelationInAct.cs

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    /// <summary>
    /// Связь работ и ЗИП в конкретном акте диагностики
    /// </summary>
    [Table("Связь работ и ЗИП в акте")]
    public class WorkSpareRelationInAct
    {
        [Key]
        [Column("Код записи")]
        public int Id { get; set; }

        [Column("Код акта")]
        [Required]
        [ForeignKey(nameof(DiagnosticAct))]
        public int ActCode { get; set; }

        [Column("Код работы")]
        [Required]
        [ForeignKey(nameof(Work))]
        public int WorkCode { get; set; }

        [Column("Код ЗИП")]
        [Required]
        [ForeignKey(nameof(Spare))]
        public int SpareCode { get; set; }

        [Column("Количество")]
        [Required]
        public int Quantity { get; set; } = 1;

        [Column("Обязательный")]
        public bool IsRequired { get; set; } = true;

        // Навигационные свойства
        public virtual DiagnosticAct? DiagnosticAct { get; set; }
        public virtual WorkEntity? Work { get; set; }
        public virtual SpareEntity? Spare { get; set; }
    }
}