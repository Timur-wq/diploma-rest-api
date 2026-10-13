using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Подписи")]
    public class SignatureEntity
    {
        [Key]
        [Column("Id подписей")]
        public int Id { get; set; }

        [Column("Подписан клиентом")]
        public bool IsSignedByClient { get; set; }

        [Column("Дата подписания")]
        public DateTime? SignedAt { get; set; }

        [Column("Метод подписания")]
        public string? SignatureMethod { get; set; }
        // "office" (лично в офисе), "online" (в ЛК), "sms_code", "click"

        [Column("Данные верификации клиента")]
        public string? ClientVerificationData { get; set; }

        // 🔥 Подпись диспетчера
        [Column("Подписан диспетчером")]
        public bool IsSignedByDispatcher { get; set; }

        [Column("Дата подписания диспетчером")]
        public DateTime? DispatcherSignedAt { get; set; }

        [Column("Табельный номер диспетчера")]
        public int? DispatcherSignedByTabNumber { get; set; }

        [Column("Данные верификации диспетчера")]
        public string? DispatcherVerificationData { get; set; }

        [Column("Верифицирован диспетчером")]
        public bool IsVerifiedByDispatcher { get; set; }

        public virtual ContractEntity? Contract { get; set; }

        public ICollection<RepairActEntity> RepairActEntities { get; set; }
            = new List<RepairActEntity>();


    }
}
