using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Клиенты")]
    public class ClientEntity
    {
        [Key]
        [Column("Код клиента")]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //[Column("ФИО клиента", TypeName = "character varying(50)")]
        //[MaxLength(50)]
        //public string FIO { get; set; }

        [Column("Контактный телефон клиента", TypeName = "character(18)")]
        [MaxLength(18)]
        public string Phone { get; set; }

        //[Column("Логин")]
        //[StringLength(20, MinimumLength = 3)]
        //[Required]
        //public string Login { get; set; } = string.Empty;

        //[Column("Пароль")]
        //[StringLength(20)]
        //[Required]
        //public string Password { get; set; } = string.Empty;

        //Активен ли аккаунт
        //[Column("Активен")]
        //public bool IsActive { get; set; } = true;

        //[Column("Дата последнего входа")]
        //public DateTime? LastLoginAt { get; set; }

        //[Column("Неудачные попытки входа")]
        //public int FailedLoginAttempts { get; set; } = 0;
        //[Column("Заблокирован до")]
        //public DateTime? LockedUntil { get; set; }

        public ICollection<RequestEntity> requestEntities { get; set; }
        //public ICollection<RefreshTokenEntity> RefreshTokens { get; set; }
        //= new List<RefreshTokenEntity>();

        [Column("Id пользователя")]
        public int? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual UserEntity? User { get; set; }

    }
}
