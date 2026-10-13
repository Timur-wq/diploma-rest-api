using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Diploma.Core.Enums;
using Microsoft.EntityFrameworkCore;  

namespace Diploma.DataAccess.Entities
{
    [Table("Сотрудники")]
    public class StafEntity
    {
        [Key]
        [Column("Табельный номер")]
        public int TabNum { get; set; }

        //[Column("Должность")]
        //[StringLength(50)]
        //public string Post { get; set; } = string.Empty;

        [Column("Дата приёма в штат", TypeName = "date")]
        public DateOnly DateStartJob { get; set; }

        //[Column("Логин")]
        //[StringLength(20, MinimumLength = 3)]
        //[Required]
        //public string Login { get; set; } = string.Empty;

        //[Column("Пароль")]
        //[StringLength(20)]  
        //[Required]
        //public string Password { get; set; } = string.Empty;  

        //Роль для авторизации
        [Column("Роль")]
        [StringLength(50)]
        [Required]
        public UserRole Role { get; set; } = UserRole.ServiceEngineer;

        //Активен ли аккаунт
        //[Column("Активен")]
        //public bool IsActive { get; set; } = true;

        //[Column("Дата последнего входа")]
        //public DateTime? LastLoginAt { get; set; }

        //[Column("Неудачные попытки входа")]
        //public int FailedLoginAttempts { get; set; } = 0;
        //[Column("Заблокирован до")]
        //public DateTime? LockedUntil { get; set; }
        //public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();

        public ICollection<StafCertificate> StafCertificates { get; set; }
            = new List<StafCertificate>();

        public ICollection<TaskEntity> Tasks { get; set; }
            = new List<TaskEntity>();

        [Column("Id пользователя")]
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity? User { get; set; }
    }
}