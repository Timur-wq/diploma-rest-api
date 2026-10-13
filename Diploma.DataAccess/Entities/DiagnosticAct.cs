// Diploma.DataAccess/Entities/DiagnosticAct.cs

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Diploma.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Diploma.DataAccess.Entities
{
    [Table("Акт диагностики")]
    [Index(nameof(TaskId), IsUnique = true)]
    public class DiagnosticAct
    {
        [Key]
        [Column("Код акта")]
        public int ActCode { get; set; }

        [Column("Код наряда")]
        [Required]
        public int TaskId { get; set; }

        [Column("Дата диагностики", TypeName = "date")]
        [Required]
        public DateOnly DateOnly { get; set; }

        [Column("Время диагностики", TypeName = "time")]
        public TimeOnly TimeOnly { get; set; } = new(0, 0);

        [Column("Внешнее состояние")]
        [StringLength(2000)]
        public string ExternalCondition { get; set; } = string.Empty;

        [Column("Выявленные неисправности")]
        [StringLength(2000)]
        public string IdentifiedIssues { get; set; } = string.Empty;

        [Column("Результаты тестов")]
        [StringLength(2000)]
        public string TestResults { get; set; } = string.Empty;

        [Column("Рекомендации")]
        [StringLength(2000)]
        public string Recommendations { get; set; } = string.Empty;

        [Column("Примерная стоимость", TypeName = "decimal(10,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column("Примерное время")]
        [StringLength(50)]
        public string EstimatedTime { get; set; } = string.Empty;

        // 🔥 Поля для согласования
        [Column("Статус акта")]
        [StringLength(20)]
        public DiagnosticActStatus Status { get; set; } = DiagnosticActStatus.Draft;

        [Column("Отправлен диспетчеру")]
        public DateTime? SubmittedToDispatcherAt { get; set; }

        [Column("Отправлен клиенту")]
        public DateTime? SentToClientAt { get; set; }

        [Column("Согласован клиентом")]
        public DateTime? ApprovedByClientAt { get; set; }

        [Column("Комментарий диспетчера")]
        [StringLength(500)]
        public string? DispatcherComment { get; set; }

        [Column("Комментарий клиента")]
        [StringLength(500)]
        public string? ClientComment { get; set; }

        [Column("Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Дата обновления")]
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        [ForeignKey(nameof(TaskId))]
        public virtual TaskEntity? Task { get; set; }

        public virtual ICollection<SparePartsDiagnosticAct> SpareParts { get; set; }
        public virtual ICollection<WorksDiagnosticAct> Works { get; set; }
        public ICollection<WorkSpareRelationInAct> WorkSpareRelations { get; set; } = new List<WorkSpareRelationInAct>();

        public ICollection<SpareOrderEntity> SpareOrderEntities { get; set; } = new List<SpareOrderEntity>();
    }
}