using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Поставщики")]
    public class SupplierEntity
    {
        [Key]
        [Column("Код поставщика")]
        public int SupplierId { get; set; }

        [Column("Наименование")]
        [StringLength(200)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [Column("Юридический адрес")]
        [StringLength(500)]
        public string? LegalAddress { get; set; }

        [Column("ИНН")]
        [StringLength(12)]
        public string? INN { get; set; }

        [Column("КПП")]
        [StringLength(9)]
        public string? KPP { get; set; }

        [Column("Адрес")]
        [StringLength(500)]
        public string? Address { get; set; }

        [Column("Телефон")]
        [StringLength(20)]
        public string? Phone { get; set; }

        public ICollection<SupplierSpareEntity> SupplierSpareEntities { get; set; }
    }
}
