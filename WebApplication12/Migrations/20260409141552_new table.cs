using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class newtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Связь работ и ЗИП",
                columns: table => new
                {
                    Кодзаписи = table.Column<int>(name: "Код записи", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Кодработы = table.Column<int>(name: "Код работы", type: "integer", nullable: false),
                    КодЗИП = table.Column<int>(name: "Код ЗИП", type: "integer", nullable: false),
                    Обязательный = table.Column<bool>(type: "boolean", nullable: false),
                    Количествопоумолчанию = table.Column<int>(name: "Количество по умолчанию", type: "integer", nullable: false),
                    Описаниесвязи = table.Column<string>(name: "Описание связи", type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Связь работ и ЗИП", x => x.Кодзаписи);
                    table.ForeignKey(
                        name: "FK_Связь работ и ЗИП_Кодификатор работ_Код работы",
                        column: x => x.Кодработы,
                        principalTable: "Кодификатор работ",
                        principalColumn: "Код работы",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Связь работ и ЗИП_Учёт ЗИП_Код ЗИП",
                        column: x => x.КодЗИП,
                        principalTable: "Учёт ЗИП",
                        principalColumn: "Код ЗИП",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Связь работ и ЗИП_Код ЗИП",
                table: "Связь работ и ЗИП",
                column: "Код ЗИП");

            migrationBuilder.CreateIndex(
                name: "IX_Связь работ и ЗИП_Код работы",
                table: "Связь работ и ЗИП",
                column: "Код работы");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Связь работ и ЗИП");
        }
    }
}
