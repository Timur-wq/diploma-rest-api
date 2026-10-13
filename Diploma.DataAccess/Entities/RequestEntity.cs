using Diploma.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    [Table("Заявки на ТО СВТ")]
    public class RequestEntity
    {
        [Key]
        [Column("Код заявки")]
        public int Id { get; set; }

        [Column("Код клиента")]
        public int ClientId { get; set; }

        [ForeignKey(nameof(ClientId))]
        public ClientEntity ClientEntity { get; set; } = null!;

        [Column("Дата поступления заявки", TypeName = "date")]
        public DateOnly DateOnly { get; set; }

        [Column("Время поступления заявки", TypeName = "time")]
        public TimeOnly TimeOnly { get; set; }

        [Column("Серийный номер СВТ", TypeName = "character varying(20)")]
        [MaxLength(20)]
        public string SerialNumber { get; set; } = string.Empty;

        [Column("Тип СВТ", TypeName = "character varying(20)")]
        [MaxLength(20)]
        public string SvtType { get; set; } = string.Empty;

        [Column("Модель СВТ", TypeName = "character varying(30)")]
        [MaxLength(30)]
        public string Model { get; set; } = string.Empty;

        [Column("Описание неисправности", TypeName = "character varying(2000)")]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Column("Статус")]
        public RequestStatus Status { get; set; } = RequestStatus.New;

        [Column("Причина отклонения")]
        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();

        public virtual ContractEntity? Contract { get; set; }

        public ICollection<InvoiceEntity>? Invoices { get; set; } = new List<InvoiceEntity>();

        public ICollection<WarrantyCardEntity> WarrantyCardEntities { get; set; } = new List<WarrantyCardEntity>();

        


    }
}