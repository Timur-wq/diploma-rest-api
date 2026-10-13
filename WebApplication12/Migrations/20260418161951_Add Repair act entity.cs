using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRepairactentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Путь к файлу",
                table: "Акт списания ЗИП",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Акт выполненных работ",
                columns: table => new
                {
                    Кодакта = table.Column<int>(name: "Код акта", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Коднаряда = table.Column<int>(name: "Код наряда", type: "integer", nullable: true),
                    Датасоздания = table.Column<DateTime>(name: "Дата создания", type: "timestamp with time zone", nullable: false),
                    Путькфайлу = table.Column<string>(name: "Путь к файлу", type: "text", nullable: false),
                    Комментарий = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Акт выполненных работ", x => x.Кодакта);
                    table.ForeignKey(
                        name: "FK_Акт выполненных работ_Наряд_Код наряда",
                        column: x => x.Коднаряда,
                        principalTable: "Наряд",
                        principalColumn: "Код наряда");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Акт выполненных работ_Код наряда",
                table: "Акт выполненных работ",
                column: "Код наряда");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "Путь к файлу",
                table: "Акт списания ЗИП");
        }
    }
}
