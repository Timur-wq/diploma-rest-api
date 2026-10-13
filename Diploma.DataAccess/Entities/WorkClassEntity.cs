using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Классификатор работ")]
    public class WorkClassEntity
    {
        [Key]
        [Column("Код класса работ")]
        public int WorkClassCode { get; set; }

        [Column("Наименование класса работ", TypeName = "character varying(20)")]
        public string WorkClassName { get; set; }

        public ICollection<WorkEntity> workEntities { get; set; } = new List<WorkEntity>();

        public ICollection<WorkClassAccessEntity> WorkClassAccessEntities { get; set; } = new List<WorkClassAccessEntity>();
    }
}
