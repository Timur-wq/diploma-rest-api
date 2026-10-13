using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class migration7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Сотрудники",
                table: "Сотрудники");

            migrationBuilder.AlterColumn<string>(
                name: "Роль",
                table: "Сотрудники",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "ФИО сотрудника",
                table: "Сотрудники",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "Табельный номер",
                table: "Сотрудники",
                column: "Табельный номер");

            migrationBuilder.CreateIndex(
                name: "IX_Сотрудники_Роль",
                table: "Сотрудники",
                column: "Роль");
        }
    }
}
