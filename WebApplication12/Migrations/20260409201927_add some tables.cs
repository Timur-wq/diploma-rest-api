using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class addsometables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Заявки на ЗИП",
                columns: table => new
                {
                    КодзаявкинаЗИП = table.Column<int>(name: "Код заявки на ЗИП", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    КодЗИП = table.Column<int>(name: "Код ЗИП", type: "integer", nullable: false),
                    ПроизводительЗИП = table.Column<string>(name: "Производитель ЗИП", type: "character varying(200)", maxLength: 200, nullable: true),
                    Количество = table.Column<int>(type: "integer", nullable: false),
                    Кодпоставщика = table.Column<int>(name: "Код поставщика", type: "integer", nullable: false),
                    СтоимостьединицыЗИП = table.Column<decimal>(name: "Стоимость единицы ЗИП", type: "numeric(10,2)", nullable: false),
                    Статус = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Заявки на ЗИП", x => x.КодзаявкинаЗИП);
                    table.ForeignKey(
                        name: "FK_Заявки на ЗИП_Поставщики_Код поставщика",
                        column: x => x.Кодпоставщика,
                        principalTable: "Поставщики",
                        principalColumn: "Код поставщика",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Заявки на ЗИП_Учёт ЗИП_Код ЗИП",
                        column: x => x.КодЗИП,
                        principalTable: "Учёт ЗИП",
                        principalColumn: "Код ЗИП",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_Код ЗИП",
                table: "Заявки на ЗИП",
                column: "Код ЗИП");

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_Код поставщика",
                table: "Заявки на ЗИП",
                column: "Код поставщика");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Заявки на ЗИП");
        }
    }
}
