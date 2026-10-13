using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diploma.DataAccess.Entities
{
    [Table("Наряд")]
    public class TaskEntity
    {

        [Key]
        [Column("Код наряда")]
        public int WorkOrderId { get; set; }

        [Column("Табельный номер сервисного инженера")]//1
        [Required]
        public int ServiceEngineerTabNumber { get; set; }

        [Column("Код работы")]//2
        [Required]
        public int WorkId { get; set; }

        [Column("Код заявки")]//3
        [Required]
        public int RequestId { get; set; }

        [Column("Дата и время назначения наряда")]
        [Required]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Column("Дата и время приступления к наряду")]
        public DateTime? StartedAt { get; set; }

        [Column("Дата и время завершения наряда")]
        public DateTime? CompletedAt { get; set; }

        [Column("Статус")]
        [StringLength(30)]
        public string Status { get; set; } = "assigned";

        [ForeignKey(nameof(ServiceEngineerTabNumber))]
        public StafEntity? StafEntity { get; set; }

        [ForeignKey(nameof(WorkId))]
        public WorkEntity? WorkEntity { get; set; }

        [ForeignKey(nameof(RequestId))]
        public RequestEntity? RequestEntity { get; set; }

        [NotMapped]
        public string StatusDisplayName => Status switch
        {
            "assigned" => "Назначен",
            "in_progress" => "В работе",
            "completed" => "Завершён",
            "cancelled" => "Отменён",
            _ => Status
        };

        [NotMapped]
        public bool IsActive => Status != "completed" && Status != "cancelled";

        public virtual ICollection<DiagnosticAct> DiagnosticActs { get; set; }
        public ICollection<SparePartsWriteOffEntity> SparePartsWriteOffEntities { get; set; }

        public ICollection<RepairActEntity> RepairActEntities { get; set; }
    }
}
