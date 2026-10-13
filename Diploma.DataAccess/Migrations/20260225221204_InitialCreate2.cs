using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Клиенты",
                columns: table => new
                {
                    Кодклиента = table.Column<int>(name: "Код клиента", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ФИОклиента = table.Column<string>(name: "ФИО клиента", type: "character varying(50)", maxLength: 50, nullable: false),
                    Контактныйтелефонклиента = table.Column<string>(name: "Контактный телефон клиента", type: "character(18)", maxLength: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Код клиента", x => x.Кодклиента);
                });

            migrationBuilder.CreateTable(
                name: "Заявки на ТО СВТ",
                columns: table => new
                {
                    Кодзаявки = table.Column<int>(name: "Код заявки", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Кодклиента = table.Column<int>(name: "Код клиента", type: "integer", nullable: false),
                    Датапоступлениязаявки = table.Column<DateOnly>(name: "Дата поступления заявки", type: "date", nullable: false),
                    Времяпоступлениязаявки = table.Column<TimeOnly>(name: "Время поступления заявки", type: "time", nullable: false),
                    СерийныйномерСВТ = table.Column<string>(name: "Серийный номер СВТ", type: "character varying(20)", maxLength: 20, nullable: false),
                    ТипСВТ = table.Column<string>(name: "Тип СВТ", type: "character varying(20)", maxLength: 20, nullable: false),
                    Модель = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Описаниенеисправности = table.Column<string>(name: "Описание неисправности", type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("Код заявки", x => x.Кодзаявки);
                    table.ForeignKey(
                        name: "FK_Заявки на ТО СВТ_Клиенты_Код клиента",
                        column: x => x.Кодклиента,
                        principalTable: "Клиенты",
                        principalColumn: "Код клиента",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ТО СВТ_Код клиента",
                table: "Заявки на ТО СВТ",
                column: "Код клиента");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Заявки на ТО СВТ");

            migrationBuilder.DropTable(
                name: "Клиенты");
        }
    }
}
