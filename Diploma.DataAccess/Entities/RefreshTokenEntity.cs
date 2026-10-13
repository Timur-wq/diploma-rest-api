// Diploma.DataAccess/Entities/RefreshTokenEntity.cs

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace Diploma.DataAccess.Entities
{
    [Table("Refresh-токены")]
    public class RefreshTokenEntity
    {
        [Key]
        public int Id { get; set; }

        // 🔥 Универсальный ID (TabNum для стафа, Id для клиента)
        [Column("Id пользователя")]
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity? User { get; set; }



        // 🔥 Тип пользователя: "Staff" или "Client"
        //[Column("Тип пользователя")]
        //[StringLength(20)]
        //[Required]
        //public string UserType { get; set; } = string.Empty;

        [Column("Токен")]
        [StringLength(256)]
        [Required]
        public string Token { get; set; } = string.Empty;

        [Column("Истекает")]
        public DateTime ExpiresAt { get; set; }

        [Column("Создан")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Использован")]
        public bool IsUsed { get; set; }

        [Column("Отозван")]
        public bool IsRevoked { get; set; }

        [Column("IP адрес")]
        [StringLength(45)]
        public string? CreatedByIp { get; set; }

        [Column("User агент")]
        [StringLength(500)]
        public string? CreatedByUserAgent { get; set; }
    }
}