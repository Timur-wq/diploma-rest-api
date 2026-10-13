using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    [Table("Отчёты")]
    public class ReportEntity
    {
        [Key]
        [Column("Код отчёта")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Автоинкремент
        public int ReportCode { get; set; }

        [Column("Дата формирования отчёта")]
        public DateTime GeneratedAt { get; set; }

        [Column("Путь к файлу")]
        public string FilePath { get; set; } = string.Empty;

        [Column("Код заявки на ТО СВТ")]
        public int RequestId { get; set; }

        // Навигация
        [ForeignKey(nameof(RequestId))]
        public virtual RequestEntity? Request { get; set; }
    }
}