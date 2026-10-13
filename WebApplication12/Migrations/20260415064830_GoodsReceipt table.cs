using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class GoodsReceipttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Заявки на ЗИП_Код ЗИП у поставщика",
                table: "Заявки на ЗИП");

            migrationBuilder.AddColumn<int>(
                name: "Код приходной накладной",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Фактически получено",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Дата списания",
                table: "Акт списания ЗИП",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Код приходной накладной",
                table: "Акт списания ЗИП",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Комментарий",
                table: "Акт списания ЗИП",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Тип списания",
                table: "Акт списания ЗИП",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Приходные накладные",
                columns: table => new
                {
                    Кодприходнойнакладной = table.Column<int>(name: "Код приходной накладной", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    КодзаявкинаЗИП = table.Column<int>(name: "Код заявки на ЗИП", type: "integer", nullable: false),
                    Заказаноколичество = table.Column<int>(name: "Заказано количество", type: "integer", nullable: false),
                    Принятоколичество = table.Column<int>(name: "Принято количество", type: "integer", nullable: false),
                    Годныеколичество = table.Column<int>(name: "Годные количество", type: "integer", nullable: false),
                    Бракколичество = table.Column<int>(name: "Брак количество", type: "integer", nullable: false),
                    Комментарий = table.Column<string>(type: "text", nullable: true),
                    Датаприёмки = table.Column<DateTime>(name: "Дата приёмки", type: "timestamp with time zone", nullable: false),
                    Принялсотрудник = table.Column<int>(name: "Принял сотрудник", type: "integer", nullable: true),
                    Статусприёмки = table.Column<int>(name: "Статус приёмки", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Приходные накладные", x => x.Кодприходнойнакладной);
                    table.ForeignKey(
                        name: "FK_Приходные накладные_Заявки на ЗИП_Код заявки на ЗИП",
                        column: x => x.КодзаявкинаЗИП,
                        principalTable: "Заявки на ЗИП",
                        principalColumn: "Код заявки на ЗИП",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Приходные накладные_Сотрудники_Принял сотрудник",
                        column: x => x.Принялсотрудник,
                        principalTable: "Сотрудники",
                        principalColumn: "Табельный номер");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_Код приходной накладной",
                table: "Заявки на ЗИП",
                column: "Код приходной накладной");

            migrationBuilder.CreateIndex(
                name: "UQ_Заявки_Поставщик_Статус_Акт",
                table: "Заявки на ЗИП",
                columns: new[] { "Код ЗИП у поставщика", "Статус", "Код акта" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Акт списания ЗИП_Код приходной накладной",
                table: "Акт списания ЗИП",
                column: "Код приходной накладной");

            migrationBuilder.CreateIndex(
                name: "IX_Приходные накладные_Код заявки на ЗИП",
                table: "Приходные накладные",
                column: "Код заявки на ЗИП");

            migrationBuilder.CreateIndex(
                name: "IX_Приходные накладные_Принял сотрудник",
                table: "Приходные накладные",
                column: "Принял сотрудник");

            migrationBuilder.AddForeignKey(
                name: "FK_Акт списания ЗИП_Приходные накладные_Код приходной накладной",
                table: "Акт списания ЗИП",
                column: "Код приходной накладной",
                principalTable: "Приходные накладные",
                principalColumn: "Код приходной накладной");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_Приходные накладные_Код приходной накладной",
                table: "Заявки на ЗИП",
                column: "Код приходной накладной",
                principalTable: "Приходные накладные",
                principalColumn: "Код приходной накладной",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Акт списания ЗИП_Приходные накладные_Код приходной накладной",
                table: "Акт списания ЗИП");

            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_Приходные накладные_Код приходной накладной",
                table: "Заявки на ЗИП");

            migrationBuilder.DropTable(
                name: "Приходные накладные");

            migrationBuilder.DropIndex(
                name: "IX_Заявки на ЗИП_Код приходной накладной",
                table: "Заявки на ЗИП");

            migrationBuilder.DropIndex(
                name: "UQ_Заявки_Поставщик_Статус_Акт",
                table: "Заявки на ЗИП");

            migrationBuilder.DropIndex(
                name: "IX_Акт списания ЗИП_Код приходной накладной",
                table: "Акт списания ЗИП");

            migrationBuilder.DropColumn(
                name: "Код приходной накладной",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "Фактически получено",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "Дата списания",
                table: "Акт списания ЗИП");

            migrationBuilder.DropColumn(
                name: "Код приходной накладной",
                table: "Акт списания ЗИП");

            migrationBuilder.DropColumn(
                name: "Комментарий",
                table: "Акт списания ЗИП");

            migrationBuilder.DropColumn(
                name: "Тип списания",
                table: "Акт списания ЗИП");

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_Код ЗИП у поставщика",
                table: "Заявки на ЗИП",
                column: "Код ЗИП у поставщика");
        }
    }
}
