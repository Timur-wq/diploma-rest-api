using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    [Table("Сотрудники-Сертификаты")]
    [Index(nameof(StafId), nameof(CertificateId), IsUnique = true, Name = "UQ_StafCert_Unique")]
    public class StafCertificate
    {
        [Key]
        [Column("Код записи")]
        public int Id { get; set; }

        [Column("Код сервисного инженера")]
        [Required]
        public int StafId { get; set; }  // ← Свойство-ключ

        [Column("Код сертификата")]
        [Required]
        public int CertificateId { get; set; }  // ← Свойство-ключ

        // ✅ [ForeignKey] на НАВИГАЦИОННЫХ свойствах, ссылается на свойства-ключи
        [ForeignKey(nameof(StafId))]
        public virtual StafEntity? Staf { get; set; }

        [ForeignKey(nameof(CertificateId))]
        public virtual CertificateEntity? Certificate { get; set; }

        [Column("Номер сертификата")]
        [StringLength(100)]
        [Required]
        public string CertificateNumber { get; set; } = string.Empty;

        [Column("Дата получения")]
        [Required]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [Column("Дата истечения")]
        public DateTime? ExpirationDate { get; set; }

        [Column("Статус")]
        [StringLength(20)]
        public string Status { get; set; } = "active";

        [NotMapped]
        public bool IsActive => Status == "active" && (!ExpirationDate.HasValue || ExpirationDate.Value > DateTime.UtcNow);

        [NotMapped]
        public int? DaysUntilExpiration => ExpirationDate.HasValue
            ? (int)(ExpirationDate.Value - DateTime.UtcNow).TotalDays : null;

        [NotMapped]
        public bool NeedsRenewal => ExpirationDate.HasValue
            && ExpirationDate.Value <= DateTime.UtcNow.AddDays(90)
            && ExpirationDate.Value > DateTime.UtcNow;

        public virtual ICollection<WorkClassAccessEntity> WorkClassAccessEntities { get; set; }
            = new List<WorkClassAccessEntity>();
    }
}