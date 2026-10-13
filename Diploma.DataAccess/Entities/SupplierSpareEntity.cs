// Diploma.DataAccess/Entities/SupplierSpareEntity.cs

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diploma.DataAccess.Entities
{
    /// <summary>
    /// Запчасти (ЗИП) у поставщиков - актуальные остатки и цены
    /// </summary>
    [Table("ЗИП у поставщиков")]
    [Index(nameof(SpareCode), nameof(SupplierId), IsUnique = true)]
    public class SupplierSpareEntity
    {
        [Key]
        [Column("Код ЗИП-Поставщик")]
        public int Id { get; set; }

        [Column("Код ЗИП")]
        [Required]
        [ForeignKey(nameof(Spare))]
        public int SpareCode { get; set; }

        [Column("Код поставщика")]
        [Required]
        [ForeignKey(nameof(Supplier))]
        public int SupplierId { get; set; }

        [Column("Цена за единицу", TypeName = "decimal(10,2)")]
        [Required]
        public decimal UnitPrice { get; set; }

        [Column("Количество в наличии")]
        [Required]
        public int QuantityInStock { get; set; } = 0;

        // Навигационные свойства
        public virtual SpareEntity? Spare { get; set; }
        public virtual SupplierEntity? Supplier { get; set; }

        public ICollection<SpareOrderEntity> SpareOrderEntities { get; set; }

        public ICollection<SparePartsWriteOffEntity> SparePartsWriteOffEntities { get; set; }

        
    }
}