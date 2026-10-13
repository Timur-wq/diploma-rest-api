using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class addtableModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Модели СВТ",
                columns: table => new
                {
                    КодмоделиСВТ = table.Column<int>(name: "Код модели СВТ", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Наименованиемодели = table.Column<string>(name: "Наименование модели", type: "character varying(30)", maxLength: 30, nullable: false),
                    Типмодели = table.Column<string>(name: "Тип модели", type: "character varying(30)", maxLength: 30, nullable: false),
                    Производитель = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Модели СВТ", x => x.КодмоделиСВТ);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Модели СВТ");
        }
    }
}
