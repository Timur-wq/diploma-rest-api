using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Addsmth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Счета_Код заявки",
                table: "Счета");

            migrationBuilder.CreateIndex(
                name: "IX_Счета_Код заявки",
                table: "Счета",
                column: "Код заявки");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Счета_Код заявки",
                table: "Счета");

            migrationBuilder.CreateIndex(
                name: "IX_Счета_Код заявки",
                table: "Счета",
                column: "Код заявки",
                unique: true);
        }
    }
}
