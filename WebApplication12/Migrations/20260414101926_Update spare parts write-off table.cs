using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Updatesparepartswriteofftable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Код наряда",
                table: "Акт списания ЗИП",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Акт списания ЗИП_Код наряда",
                table: "Акт списания ЗИП",
                column: "Код наряда");

            migrationBuilder.AddForeignKey(
                name: "FK_Акт списания ЗИП_Наряд_Код наряда",
                table: "Акт списания ЗИП",
                column: "Код наряда",
                principalTable: "Наряд",
                principalColumn: "Код наряда",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Акт списания ЗИП_Наряд_Код наряда",
                table: "Акт списания ЗИП");

            migrationBuilder.DropIndex(
                name: "IX_Акт списания ЗИП_Код наряда",
                table: "Акт списания ЗИП");

            migrationBuilder.DropColumn(
                name: "Код наряда",
                table: "Акт списания ЗИП");
        }
    }
}
