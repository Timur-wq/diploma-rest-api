using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Сертификаты")]
    public class CertificateEntity
    {
        [Key]
        [Column("Код сертификата")]
        public int CertificateId { get; set; }

        [Column("Наименование")]
        [StringLength(300)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [Column("Эмитент")]
        [StringLength(200)]
        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Column("Тип")]
        [StringLength(50)]
        [Required]
        public string Type { get; set; } = string.Empty;

        [Column("Категория")]
        [StringLength(100)]
        public string? Category { get; set; }

        [Column("Описание")]
        public string? Description { get; set; }

        [Column("Срок действия месяцев")]
        public int? ValidityPeriodMonths { get; set; }

        [Column("Активен")]
        public bool IsActive { get; set; } = true;

        [Column("Необходимость продления")]
        public bool RequiresRenewal { get; set; } = true;

        [NotMapped]
        public bool IsLifetime => !ValidityPeriodMonths.HasValue;

        //для свойств not mapped столбец в таблице не создаётся
        [NotMapped]
        public string TypeDisplayName => Type switch
        {
            "vendor" => "Вендорный",
            "industry" => "Отраслевой",
            "internal" => "Внутренний",
            _ => Type
        };

        [NotMapped]
        public string CategoryDisplayName => Category switch
        {
            "hardware" => "Аппаратное обеспечение",
            "software" => "Программное обеспечение",
            "mobile" => "Мобильные устройства",
            "network" => "Сети",
            "security" => "Безопасность",
            _ => Category ?? "Другое"
        };

        public ICollection<StafCertificate> StafCertificates { get; set; }
            = new List<StafCertificate>();

    }
}
