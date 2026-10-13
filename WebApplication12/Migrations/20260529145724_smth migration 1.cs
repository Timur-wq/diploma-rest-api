using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class smthmigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UQ_Заявки_Поставщик_Статус_Акт",
                table: "Заявки на ЗИП",
                newName: "IX_Заявки на ЗИП_Код ЗИП у поставщика_Статус_Код акта");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Заявки на ЗИП_Код ЗИП у поставщика_Статус_Код акта",
                table: "Заявки на ЗИП",
                newName: "UQ_Заявки_Поставщик_Статус_Акт");
        }
    }
}
