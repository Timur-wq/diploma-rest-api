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
    [Table("Допуск к классам работ")]
    [PrimaryKey(nameof(WorkClassCode), nameof(StafCertificateId))]
    public class WorkClassAccessEntity
    {
        [Key]
        [Column("Код класса работ")]
        [Required]
        [ForeignKey(nameof(WorkClass))]
        public int WorkClassCode { get; set; }

        [Key]
        [Column("Код записи сертификата сотрудника")]
        [Required]
        [ForeignKey(nameof(StafCertificate))]
        public int StafCertificateId { get; set; }

        // ✅ Навигационные свойства
        public virtual WorkClassEntity? WorkClass { get; set; }
        public virtual StafCertificate? StafCertificate { get; set; }
    }
}
