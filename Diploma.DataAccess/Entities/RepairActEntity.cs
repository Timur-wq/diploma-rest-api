using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Акт выполненных работ")]
    public class RepairActEntity
    {
        [Key]
        [Column("Код акта")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("Код наряда")]
        public int? TaskId { get; set; }

        [Column("Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Путь к файлу")]
        public string FilePath { get; set; } = string.Empty;

        [Column("Комментарий")]
        public string? Comment { get; set; }

        [ForeignKey(nameof(TaskId))]
        public virtual TaskEntity? Task { get; set; }

        //// 🔥 НОВОЕ: Статус подписания
        //[Column("Подписан клиентом")]
        //public bool IsSignedByClient { get; set; }

        //[Column("Дата подписания")]
        //public DateTime? SignedAt { get; set; }

        //[Column("Метод подписания")]
        //public string? SignatureMethod { get; set; }
        //// "office" (лично в офисе), "online" (в ЛК), "sms_code", "click"

        //[Column("Данные верификации клиента")]
        //public string? ClientVerificationData { get; set; }

        //// 🔥 Подпись диспетчера
        //[Column("Подписан диспетчером")]
        //public bool IsSignedByDispatcher { get; set; }

        //[Column("Дата подписания диспетчером")]
        //public DateTime? DispatcherSignedAt { get; set; }

        //[Column("Табельный номер диспетчера")]
        //public int? DispatcherSignedByTabNumber { get; set; }

        //[Column("Данные верификации диспетчера")]
        //public string? DispatcherVerificationData { get; set; }

        //[Column("Верифицирован диспетчером")]
        //public bool IsVerifiedByDispatcher { get; set; }

        // 🔥 Навигация на подпись (одна запись на договор)

        // 🔥 Свойства-обёртки для удобного доступа к данным подписи
        // Клиент
        public bool IsSignedByClient
        {
            get => Signature?.IsSignedByClient ?? false;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.IsSignedByClient = value;
            }
        }

        public DateTime? SignedAt
        {
            get => Signature?.SignedAt;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.SignedAt = value;
            }
        }

        public string? SignatureMethod
        {
            get => Signature?.SignatureMethod;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.SignatureMethod = value;
            }
        }

        public string? ClientVerificationData
        {
            get => Signature?.ClientVerificationData;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.ClientVerificationData = value;
            }
        }

        // Диспетчер
        public bool IsSignedByDispatcher
        {
            get => Signature?.IsSignedByDispatcher ?? false;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.IsSignedByDispatcher = value;
            }
        }

        public DateTime? DispatcherSignedAt
        {
            get => Signature?.DispatcherSignedAt;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.DispatcherSignedAt = value;
            }
        }

        public int? DispatcherSignedByTabNumber
        {
            get => Signature?.DispatcherSignedByTabNumber;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.DispatcherSignedByTabNumber = value;
            }
        }

        public string? DispatcherVerificationData
        {
            get => Signature?.DispatcherVerificationData;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.DispatcherVerificationData = value;
            }
        }

        public bool IsVerifiedByDispatcher
        {
            get => Signature?.IsVerifiedByDispatcher ?? false;
            set
            {
                if (Signature == null) Signature = new SignatureEntity();
                Signature.IsVerifiedByDispatcher = value;
            }
        }
        [ForeignKey(nameof(SignatureId))]
        public virtual SignatureEntity? Signature { get; set; }


        public int? SignatureId { get; set; }

        public ICollection<WarrantyCardEntity> WarrantyCardEntities { get; set; } = new List<WarrantyCardEntity>();
    }
}
