using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class c50 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Наименование работы",
                table: "Кодификатор работ",
                type: "character varying(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Наименование работы",
                table: "Кодификатор работ",
                type: "character varying(30)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)");
        }
    }
}
