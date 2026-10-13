using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Кодификатор работ")]
    public class WorkEntity
    {
        [Key]
        [Column("Код работы")]
        public int WorkCode { get; set; }

        [Column("Наименование работы", TypeName = "character varying(50)")]
        public string WorkName { get; set; }

        [Column("Код класса работ")]
        public int WorkClassCode { get; set; }

        [ForeignKey(nameof(WorkClassCode))]
        public WorkClassEntity WorkClass { get; set; } = null!;

        [Column("Стоимость работы в час")]
        public int PricePerHour { get; set; }
        public ICollection<WorksDiagnosticAct> WorkDiagnosticActs { get; set; } = new List<WorksDiagnosticAct>();
        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();

        public ICollection<WorkSpareRelationInAct> WorkSpareRelations { get; set; } = new List<WorkSpareRelationInAct>();

    }
}
