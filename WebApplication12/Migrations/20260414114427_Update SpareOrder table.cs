using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSpareOrdertable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Код акта",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_Код акта",
                table: "Заявки на ЗИП",
                column: "Код акта");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_Акт диагностики_Код акта",
                table: "Заявки на ЗИП",
                column: "Код акта",
                principalTable: "Акт диагностики",
                principalColumn: "Код акта");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_Акт диагностики_Код акта",
                table: "Заявки на ЗИП");

            migrationBuilder.DropIndex(
                name: "IX_Заявки на ЗИП_Код акта",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "Код акта",
                table: "Заявки на ЗИП");
        }
    }
}
