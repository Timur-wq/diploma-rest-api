using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Updatediagnosticact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosticAct_Request",
                table: "Акт диагностики");

            migrationBuilder.DropForeignKey(
                name: "FK_DiagnosticAct_Staf",
                table: "Акт диагностики");

            //migrationBuilder.DropIndex(
            //    name: "IX_Акт диагностики_Код заявки",
            //    table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Код заявки",
                table: "Акт диагностики");

            migrationBuilder.RenameColumn(
                name: "Табельный номер сервисного инженера",
                table: "Акт диагностики",
                newName: "Код наряда");

            //migrationBuilder.RenameIndex(
            //    name: "IX_Акт диагностики_Табельный номер сервисного инженера",
            //    table: "Акт диагностики",
            //    newName: "IX_Акт диагностики_Код наряда");

            migrationBuilder.AddForeignKey(
                name: "FK_Акт диагностики_Наряд_Код наряда",
                table: "Акт диагностики",
                column: "Код наряда",
                principalTable: "Наряд",
                principalColumn: "Код наряда",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Акт диагностики_Наряд_Код наряда",
                table: "Акт диагностики");

            migrationBuilder.RenameColumn(
                name: "Код наряда",
                table: "Акт диагностики",
                newName: "Табельный номер сервисного инженера");

            migrationBuilder.RenameIndex(
                name: "IX_Акт диагностики_Код наряда",
                table: "Акт диагностики",
                newName: "IX_Акт диагностики_Табельный номер сервисного инженера");

            migrationBuilder.AddColumn<int>(
                name: "Код заявки",
                table: "Акт диагностики",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Акт диагностики_Код заявки",
                table: "Акт диагностики",
                column: "Код заявки");

            migrationBuilder.AddForeignKey(
                name: "FK_Акт диагностики_Заявки на ТО СВТ_Код заявки",
                table: "Акт диагностики",
                column: "Код заявки",
                principalTable: "Заявки на ТО СВТ",
                principalColumn: "Код заявки",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Акт диагностики_Сотрудники_Табельный номер сервисного инжен~",
                table: "Акт диагностики",
                column: "Табельный номер сервисного инженера",
                principalTable: "Сотрудники",
                principalColumn: "Табельный номер",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
