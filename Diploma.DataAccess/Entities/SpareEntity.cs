using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Учёт ЗИП")]
    public class SpareEntity
    {
        [Key]
        [Column("Код ЗИП")]
        public int SpareCode { get; set; }

        [Column("Наименование ЗИП")]
        public string SpareName { get; set;}

        [Column("Количество на складе")]
        public int Amount { get; set;}


        public ICollection<SparePartsDiagnosticAct> SparePartsDiagnosticActs { get; set; }

        public ICollection<WorkSpareRelationInAct> WorkSpareRelations { get; set; } = new List<WorkSpareRelationInAct>();

        public ICollection<SupplierSpareEntity> SupplierSpareEntities { get; set; }
    }
}
