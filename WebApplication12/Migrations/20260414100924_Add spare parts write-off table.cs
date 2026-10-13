using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Addsparepartswriteofftable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Акт списания ЗИП",
                columns: table => new
                {
                    КодактасписанияЗИП = table.Column<int>(name: "Код акта списания ЗИП", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    КодЗИПупоставщика = table.Column<int>(name: "Код ЗИП у поставщика", type: "integer", nullable: false),
                    Списываемоеколичество = table.Column<int>(name: "Списываемое количество", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Акт списания ЗИП", x => x.КодактасписанияЗИП);
                    table.ForeignKey(
                        name: "FK_Акт списания ЗИП_ЗИП у поставщиков_Код ЗИП у поставщика",
                        column: x => x.КодЗИПупоставщика,
                        principalTable: "ЗИП у поставщиков",
                        principalColumn: "Код ЗИП-Поставщик",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Акт списания ЗИП_Код ЗИП у поставщика",
                table: "Акт списания ЗИП",
                column: "Код ЗИП у поставщика");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Акт списания ЗИП");
        }
    }
}
