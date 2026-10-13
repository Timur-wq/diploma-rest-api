using System.ComponentModel.DataAnnotations;

namespace Diploma.Core.Models
{
    /// <summary>
    /// DTO для создания заявки на ремонт
    /// </summary>
    public class RepairRequest
    {
        // ✅ Обязательный параметрless-конструктор для JSON-десериализации
        public RepairRequest() { }

        [Required(ErrorMessage = "ФИО обязательно")]
        [StringLength(50, MinimumLength = 3)]
        public string Fio { get; set; } = string.Empty;

        [Required(ErrorMessage = "Телефон обязателен")]
        [Phone(ErrorMessage = "Неверный формат телефона")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Тип СВТ обязателен")]
        public string SvtType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Модель обязательна")]
        [StringLength(100, MinimumLength = 2)]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Серийный номер обязателен")]
        [StringLength(50)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        public DateOnly DateOnly { get; set; }

        [Required]
        public TimeOnly TimeOnly { get; set; }

        [Required(ErrorMessage = "Описание обязательно")]
        [StringLength(2000, MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;
    }
}