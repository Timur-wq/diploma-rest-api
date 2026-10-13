using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReporttable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Отчёты",
                columns: table => new
                {
                    Кодотчёта = table.Column<int>(name: "Код отчёта", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Датаформированияотчёта = table.Column<DateTime>(name: "Дата формирования отчёта", type: "timestamp with time zone", nullable: false),
                    Путькфайлу = table.Column<string>(name: "Путь к файлу", type: "text", nullable: false),
                    КодзаявкинаТОСВТ = table.Column<int>(name: "Код заявки на ТО СВТ", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Отчёты", x => x.Кодотчёта);
                    table.ForeignKey(
                        name: "FK_Отчёты_Заявки на ТО СВТ_Код заявки на ТО СВТ",
                        column: x => x.КодзаявкинаТОСВТ,
                        principalTable: "Заявки на ТО СВТ",
                        principalColumn: "Код заявки",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Отчёты_Код заявки на ТО СВТ",
                table: "Отчёты",
                column: "Код заявки на ТО СВТ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Отчёты");
        }
    }
}
