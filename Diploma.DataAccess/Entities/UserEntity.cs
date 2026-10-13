// Diploma.DataAccess/Entities/UserEntity.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    [Table("Пользователи")]
    public class UserEntity
    {
        [Key]
        [Column("Id")]

        public int UserId { get; set; }

        [Column("Логин", TypeName = "character varying(20)")]
        [MaxLength(100)]
        public string Login { get; set; } = string.Empty;

        [Column("Пароль", TypeName = "character varying(20)")]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Column("UserType", TypeName = "character varying(20)")]
        [MaxLength(20)]
        public string UserType { get; set; } = string.Empty; // "client" или "staff"

        [Column("FullName", TypeName = "character varying(200)")]
        [MaxLength(200)]
        public string? FullName { get; set; }

        //Активен ли аккаунт
        [Column("Активен")]
        public bool IsActive { get; set; } = true;

        [Column("Дата последнего входа")]
        public DateTime? LastLoginAt { get; set; }

        [Column("Неудачные попытки входа")]
        public int FailedLoginAttempts { get; set; } = 0;
        [Column("Заблокирован до")]
        public DateTime? LockedUntil { get; set; }

        public ICollection<StafEntity> StafEntities { get; set; }

        public ICollection<ClientEntity> ClientEntities { get; set; }

        public ICollection<RefreshTokenEntity> RefreshTokens { get; set; } = new List<RefreshTokenEntity>();
    }
}