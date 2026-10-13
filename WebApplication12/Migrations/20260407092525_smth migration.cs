using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class smthmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<bool>(
                name: "Согласовано клиентом",
                table: "Перечень работ - Акт диагностики",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Согласовано клиентом",
                table: "Перечень ЗИП - Акт диагностики",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Внешнее состояние",
                table: "Акт диагностики",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "Время диагностики",
                table: "Акт диагностики",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "Выявленные неисправности",
                table: "Акт диагностики",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Дата обновления",
                table: "Акт диагностики",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Дата создания",
                table: "Акт диагностики",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Комментарий диспетчера",
                table: "Акт диагностики",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Комментарий клиента",
                table: "Акт диагностики",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Отправлен диспетчеру",
                table: "Акт диагностики",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Отправлен клиенту",
                table: "Акт диагностики",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Примерная стоимость",
                table: "Акт диагностики",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Примерное время",
                table: "Акт диагностики",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Результаты тестов",
                table: "Акт диагностики",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Рекомендации",
                table: "Акт диагностики",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Согласован клиентом",
                table: "Акт диагностики",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Статус акта",
                table: "Акт диагностики",
                type: "integer",
                maxLength: 20,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Акт диагностики_Код наряда",
                table: "Акт диагностики",
                column: "Код наряда",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_User",
                table: "Refresh-токены",
                columns: new[] { "Код пользователя", "Тип пользователя" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Акт диагностики_Код наряда",
                table: "Акт диагностики");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_User",
                table: "Refresh-токены");

            migrationBuilder.DropColumn(
                name: "Согласовано клиентом",
                table: "Перечень работ - Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Согласовано клиентом",
                table: "Перечень ЗИП - Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Внешнее состояние",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Время диагностики",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Выявленные неисправности",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Дата обновления",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Дата создания",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Комментарий диспетчера",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Комментарий клиента",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Отправлен диспетчеру",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Отправлен клиенту",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Примерная стоимость",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Примерное время",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Результаты тестов",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Рекомендации",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Согласован клиентом",
                table: "Акт диагностики");

            migrationBuilder.DropColumn(
                name: "Статус акта",
                table: "Акт диагностики");

            migrationBuilder.CreateIndex(
                name: "IX_Акт диагностики_Код наряда",
                table: "Акт диагностики",
                column: "Код наряда");
        }
    }
}
