using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Updatetablename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Счета на предоплату_Заявки на ТО СВТ_Код заявки",
                table: "Счета на предоплату");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Счета на предоплату",
                table: "Счета на предоплату");

            migrationBuilder.RenameTable(
                name: "Счета на предоплату",
                newName: "Счета");

            migrationBuilder.RenameIndex(
                name: "IX_Счета на предоплату_Код заявки",
                table: "Счета",
                newName: "IX_Счета_Код заявки");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Счета",
                table: "Счета",
                column: "Код счёта");

            migrationBuilder.AddForeignKey(
                name: "FK_Счета_Заявки на ТО СВТ_Код заявки",
                table: "Счета",
                column: "Код заявки",
                principalTable: "Заявки на ТО СВТ",
                principalColumn: "Код заявки",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Счета_Заявки на ТО СВТ_Код заявки",
                table: "Счета");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Счета",
                table: "Счета");

            migrationBuilder.RenameTable(
                name: "Счета",
                newName: "Счета на предоплату");

            migrationBuilder.RenameIndex(
                name: "IX_Счета_Код заявки",
                table: "Счета на предоплату",
                newName: "IX_Счета на предоплату_Код заявки");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Счета на предоплату",
                table: "Счета на предоплату",
                column: "Код счёта");

            migrationBuilder.AddForeignKey(
                name: "FK_Счета на предоплату_Заявки на ТО СВТ_Код заявки",
                table: "Счета на предоплату",
                column: "Код заявки",
                principalTable: "Заявки на ТО СВТ",
                principalColumn: "Код заявки",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
