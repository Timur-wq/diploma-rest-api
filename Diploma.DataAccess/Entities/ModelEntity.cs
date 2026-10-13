using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Модели СВТ")]
    public class ModelEntity
    {
        [Key]
        [Column("Код модели СВТ")]
        public int ModelCode { get; set; }

        [Column("Наименование модели", TypeName = "character varying(30)")]
        [StringLength(30)]
        [Required]
        public string ModelName { get; set; } = string.Empty;

        [Column("Тип модели", TypeName = "character varying(30)")]
        [StringLength(30)]
        [Required]
        public string ModelType { get; set; } = string.Empty;

        [Column("Производитель", TypeName = "character varying(30)")]
        [StringLength(30)]
        [Required]
        public string Creator { get; set; } = string.Empty;

        public ICollection<CertificateModelEntity> CertificateModelEntities { get; set; }
    }
}
