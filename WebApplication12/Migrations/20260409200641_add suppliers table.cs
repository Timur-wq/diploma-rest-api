using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class addsupplierstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Поставщики",
                columns: table => new
                {
                    Кодпоставщика = table.Column<int>(name: "Код поставщика", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Наименование = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Юридическийадрес = table.Column<string>(name: "Юридический адрес", type: "character varying(500)", maxLength: 500, nullable: true),
                    ИНН = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    КПП = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    Адрес = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Телефон = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Поставщики", x => x.Кодпоставщика);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Поставщики");
        }
    }
}
