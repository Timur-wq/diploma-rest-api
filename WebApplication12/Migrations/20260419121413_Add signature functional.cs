using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Addsignaturefunctional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Верифицирован диспетчером",
                table: "Акт выполненных работ",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Дата подписания",
                table: "Акт выполненных работ",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Метод подписания",
                table: "Акт выполненных работ",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Подписан клиентом",
                table: "Акт выполненных работ",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Гарантийные талоны",
                columns: table => new
                {
                    Кодталона = table.Column<int>(name: "Код талона", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Кодзаявки = table.Column<int>(name: "Код заявки", type: "integer", nullable: false),
                    Кодакта = table.Column<int>(name: "Код акта", type: "integer", nullable: false),
                    ПутькфайлуPDF = table.Column<string>(name: "Путь к файлу PDF", type: "text", nullable: false),
                    Датавыдачи = table.Column<DateTime>(name: "Дата выдачи", type: "timestamp with time zone", nullable: false),
                    Гарантийныйсрокмес = table.Column<int>(name: "Гарантийный срок (мес)", type: "integer", nullable: false),
                    Действуетдо = table.Column<DateTime>(name: "Действует до", type: "timestamp with time zone", nullable: false),
                    Активен = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Гарантийные талоны", x => x.Кодталона);
                    table.ForeignKey(
                        name: "FK_Гарантийные талоны_Акт выполненных работ_Код акта",
                        column: x => x.Кодакта,
                        principalTable: "Акт выполненных работ",
                        principalColumn: "Код акта",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Гарантийные талоны_Заявки на ТО СВТ_Код заявки",
                        column: x => x.Кодзаявки,
                        principalTable: "Заявки на ТО СВТ",
                        principalColumn: "Код заявки",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Гарантийные талоны_Код акта",
                table: "Гарантийные талоны",
                column: "Код акта");

            migrationBuilder.CreateIndex(
                name: "IX_Гарантийные талоны_Код заявки",
                table: "Гарантийные талоны",
                column: "Код заявки");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Гарантийные талоны");

            migrationBuilder.DropColumn(
                name: "Верифицирован диспетчером",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "Дата подписания",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "Метод подписания",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "Подписан клиентом",
                table: "Акт выполненных работ");
        }
    }
}
