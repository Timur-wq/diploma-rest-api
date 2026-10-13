using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class addtableModelCertificate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Сертификаты-Модели",
                columns: table => new
                {
                    Кодсертификата = table.Column<int>(name: "Код сертификата", type: "integer", nullable: false),
                    КодмоделиСВТ = table.Column<int>(name: "Код модели СВТ", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Сертификаты-Модели", x => new { x.Кодсертификата, x.КодмоделиСВТ });
                    table.ForeignKey(
                        name: "FK_Сертификаты-Модели_Модели СВТ_Код модели СВТ",
                        column: x => x.КодмоделиСВТ,
                        principalTable: "Модели СВТ",
                        principalColumn: "Код модели СВТ",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Сертификаты-Модели_Сертификаты_Код сертификата",
                        column: x => x.Кодсертификата,
                        principalTable: "Сертификаты",
                        principalColumn: "Код сертификата",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Сертификаты-Модели_Код модели СВТ",
                table: "Сертификаты-Модели",
                column: "Код модели СВТ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Сертификаты-Модели");
        }
    }
}
