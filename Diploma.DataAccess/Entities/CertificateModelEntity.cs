using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Сертификаты-Модели")]
    [PrimaryKey(nameof(CertificateId), nameof(ModelCode))]
    public class CertificateModelEntity
    {
        [Key]
        [Column("Код сертификата")]
        [ForeignKey(nameof(Certificate))]
        public int CertificateId { get; set; }

        [Key]
        [Column("Код модели СВТ")]
        [ForeignKey(nameof(Model))]
        public int ModelCode { get; set; }

        public virtual CertificateEntity Certificate { get; set; }
        public virtual ModelEntity Model { get; set; }

        
    }
}
