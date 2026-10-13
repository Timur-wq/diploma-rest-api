using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Updateschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_Поставщики_Код поставщика",
                table: "Заявки на ЗИП");

            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_Учёт ЗИП_Код ЗИП",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "Производитель ЗИП",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "Стоимость единицы ЗИП",
                table: "Заявки на ЗИП");

            migrationBuilder.RenameColumn(
                name: "Количество",
                table: "Заявки на ЗИП",
                newName: "Закупаемое количество");

            migrationBuilder.RenameColumn(
                name: "Код поставщика",
                table: "Заявки на ЗИП",
                newName: "SupplierId");

            migrationBuilder.RenameColumn(
                name: "Код ЗИП",
                table: "Заявки на ЗИП",
                newName: "SpareCode");

            migrationBuilder.RenameIndex(
                name: "IX_Заявки на ЗИП_Код поставщика",
                table: "Заявки на ЗИП",
                newName: "IX_Заявки на ЗИП_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Заявки на ЗИП_Код ЗИП",
                table: "Заявки на ЗИП",
                newName: "IX_Заявки на ЗИП_SpareCode");

            migrationBuilder.AlterColumn<int>(
                name: "SupplierId",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "SpareCode",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "SupplierSpareEntityId",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Код ЗИП-поставщика",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ЗИП у поставщиков",
                columns: table => new
                {
                    КодЗИППоставщик = table.Column<int>(name: "Код ЗИП-Поставщик", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    КодЗИП = table.Column<int>(name: "Код ЗИП", type: "integer", nullable: false),
                    Кодпоставщика = table.Column<int>(name: "Код поставщика", type: "integer", nullable: false),
                    Ценазаединицу = table.Column<decimal>(name: "Цена за единицу", type: "numeric(10,2)", nullable: false),
                    Количествовналичии = table.Column<int>(name: "Количество в наличии", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ЗИП у поставщиков", x => x.КодЗИППоставщик);
                    table.ForeignKey(
                        name: "FK_ЗИП у поставщиков_Поставщики_Код поставщика",
                        column: x => x.Кодпоставщика,
                        principalTable: "Поставщики",
                        principalColumn: "Код поставщика",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ЗИП у поставщиков_Учёт ЗИП_Код ЗИП",
                        column: x => x.КодЗИП,
                        principalTable: "Учёт ЗИП",
                        principalColumn: "Код ЗИП",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_SupplierSpareEntityId",
                table: "Заявки на ЗИП",
                column: "SupplierSpareEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ЗИП у поставщиков_Код ЗИП_Код поставщика",
                table: "ЗИП у поставщиков",
                columns: new[] { "Код ЗИП", "Код поставщика" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ЗИП у поставщиков_Код поставщика",
                table: "ЗИП у поставщиков",
                column: "Код поставщика");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_ЗИП у поставщиков_SupplierSpareEntityId",
                table: "Заявки на ЗИП",
                column: "SupplierSpareEntityId",
                principalTable: "ЗИП у поставщиков",
                principalColumn: "Код ЗИП-Поставщик");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_Поставщики_SupplierId",
                table: "Заявки на ЗИП",
                column: "SupplierId",
                principalTable: "Поставщики",
                principalColumn: "Код поставщика");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_Учёт ЗИП_SpareCode",
                table: "Заявки на ЗИП",
                column: "SpareCode",
                principalTable: "Учёт ЗИП",
                principalColumn: "Код ЗИП");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_ЗИП у поставщиков_SupplierSpareEntityId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_Поставщики_SupplierId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_Учёт ЗИП_SpareCode",
                table: "Заявки на ЗИП");

            migrationBuilder.DropTable(
                name: "ЗИП у поставщиков");

            migrationBuilder.DropIndex(
                name: "IX_Заявки на ЗИП_SupplierSpareEntityId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "SupplierSpareEntityId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "Код ЗИП-поставщика",
                table: "Заявки на ЗИП");

            migrationBuilder.RenameColumn(
                name: "Закупаемое количество",
                table: "Заявки на ЗИП",
                newName: "Количество");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "Заявки на ЗИП",
                newName: "Код поставщика");

            migrationBuilder.RenameColumn(
                name: "SpareCode",
                table: "Заявки на ЗИП",
                newName: "Код ЗИП");

            migrationBuilder.RenameIndex(
                name: "IX_Заявки на ЗИП_SupplierId",
                table: "Заявки на ЗИП",
                newName: "IX_Заявки на ЗИП_Код поставщика");

            migrationBuilder.RenameIndex(
                name: "IX_Заявки на ЗИП_SpareCode",
                table: "Заявки на ЗИП",
                newName: "IX_Заявки на ЗИП_Код ЗИП");

            migrationBuilder.AlterColumn<int>(
                name: "Код поставщика",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Код ЗИП",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Производитель ЗИП",
                table: "Заявки на ЗИП",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Стоимость единицы ЗИП",
                table: "Заявки на ЗИП",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_Поставщики_Код поставщика",
                table: "Заявки на ЗИП",
                column: "Код поставщика",
                principalTable: "Поставщики",
                principalColumn: "Код поставщика",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_Учёт ЗИП_Код ЗИП",
                table: "Заявки на ЗИП",
                column: "Код ЗИП",
                principalTable: "Учёт ЗИП",
                principalColumn: "Код ЗИП",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
