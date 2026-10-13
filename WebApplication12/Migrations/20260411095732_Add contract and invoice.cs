using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Addcontractandinvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Договоры",
                columns: table => new
                {
                    Коддоговора = table.Column<int>(name: "Код договора", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Кодзаявки = table.Column<int>(name: "Код заявки", type: "integer", nullable: false),
                    Номердоговора = table.Column<string>(name: "Номер договора", type: "character varying(50)", maxLength: 50, nullable: true),
                    Статусдоговора = table.Column<int>(name: "Статус договора", type: "integer", nullable: false),
                    Путькфайлу = table.Column<string>(name: "Путь к файлу", type: "character varying(500)", maxLength: 500, nullable: true),
                    Датасоздания = table.Column<DateTime>(name: "Дата создания", type: "timestamp with time zone", nullable: false),
                    Датаподписания = table.Column<DateTime>(name: "Дата подписания", type: "timestamp with time zone", nullable: true),
                    Подписанклиентом = table.Column<bool>(name: "Подписан клиентом", type: "boolean", nullable: false),
                    Комментарий = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Договоры", x => x.Коддоговора);
                    table.ForeignKey(
                        name: "FK_Договоры_Заявки на ТО СВТ_Код заявки",
                        column: x => x.Кодзаявки,
                        principalTable: "Заявки на ТО СВТ",
                        principalColumn: "Код заявки",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Счета на предоплату",
                columns: table => new
                {
                    Кодсчёта = table.Column<int>(name: "Код счёта", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Кодзаявки = table.Column<int>(name: "Код заявки", type: "integer", nullable: false),
                    Номерсчёта = table.Column<string>(name: "Номер счёта", type: "character varying(50)", maxLength: 50, nullable: true),
                    Статуссчёта = table.Column<int>(name: "Статус счёта", type: "integer", nullable: false),
                    Сумма = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Путькфайлу = table.Column<string>(name: "Путь к файлу", type: "character varying(500)", maxLength: 500, nullable: true),
                    Датасоздания = table.Column<DateTime>(name: "Дата создания", type: "timestamp with time zone", nullable: false),
                    Датавыставления = table.Column<DateTime>(name: "Дата выставления", type: "timestamp with time zone", nullable: true),
                    Датаоплаты = table.Column<DateTime>(name: "Дата оплаты", type: "timestamp with time zone", nullable: true),
                    Путькчеку = table.Column<string>(name: "Путь к чеку", type: "character varying(500)", maxLength: 500, nullable: true),
                    Комментарий = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Счета на предоплату", x => x.Кодсчёта);
                    table.ForeignKey(
                        name: "FK_Счета на предоплату_Заявки на ТО СВТ_Код заявки",
                        column: x => x.Кодзаявки,
                        principalTable: "Заявки на ТО СВТ",
                        principalColumn: "Код заявки",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Договоры_Код заявки",
                table: "Договоры",
                column: "Код заявки",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Счета на предоплату_Код заявки",
                table: "Счета на предоплату",
                column: "Код заявки",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Договоры");

            migrationBuilder.DropTable(
                name: "Счета на предоплату");
        }
    }
}
