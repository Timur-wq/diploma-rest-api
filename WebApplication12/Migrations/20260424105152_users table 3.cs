using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class userstable3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 🔥 Удаляем все старые refresh-токены перед миграцией
            migrationBuilder.Sql(@"
    DELETE FROM ""Refresh-токены""
    WHERE ""Код пользователя"" IS NOT NULL
    AND NOT EXISTS (
        SELECT 1 FROM ""Пользователи"" п 
        WHERE п.""Id"" = ""Refresh-токены"".""Код пользователя""
    );
");
            migrationBuilder.DropForeignKey(
                name: "FK_Refresh-токены_Клиенты_ClientEntityId",
                table: "Refresh-токены");

            migrationBuilder.DropForeignKey(
                name: "FK_Refresh-токены_Сотрудники_StafEntityTabNum",
                table: "Refresh-токены");

            migrationBuilder.DropIndex(
                name: "IX_Refresh-токены_ClientEntityId",
                table: "Refresh-токены");

            migrationBuilder.DropIndex(
                name: "IX_Refresh-токены_StafEntityTabNum",
                table: "Refresh-токены");

            migrationBuilder.DropColumn(
                name: "Активен",
                table: "Сотрудники");

            migrationBuilder.DropColumn(
                name: "Дата последнего входа",
                table: "Сотрудники");

            migrationBuilder.DropColumn(
                name: "Заблокирован до",
                table: "Сотрудники");

            migrationBuilder.DropColumn(
                name: "Неудачные попытки входа",
                table: "Сотрудники");

            migrationBuilder.DropColumn(
                name: "Активен",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Дата последнего входа",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Заблокирован до",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Неудачные попытки входа",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "ФИО клиента",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "ClientEntityId",
                table: "Refresh-токены");

            migrationBuilder.DropColumn(
                name: "StafEntityTabNum",
                table: "Refresh-токены");

            migrationBuilder.RenameColumn(
                name: "Код пользователя",
                table: "Refresh-токены",
                newName: "Id пользователя");

            migrationBuilder.AlterColumn<int>(
                name: "Id пользователя",
                table: "Refresh-токены",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Refresh-токены_Пользователи_Id пользователя",
                table: "Refresh-токены",
                column: "Id пользователя",
                principalTable: "Пользователи",
                principalColumn: "Id");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Refresh-токены_Пользователи_Id пользователя",
                table: "Refresh-токены");

            migrationBuilder.RenameColumn(
                name: "Id пользователя",
                table: "Refresh-токены",
                newName: "Код пользователя");

            migrationBuilder.AddColumn<bool>(
                name: "Активен",
                table: "Сотрудники",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Дата последнего входа",
                table: "Сотрудники",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Заблокирован до",
                table: "Сотрудники",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Неудачные попытки входа",
                table: "Сотрудники",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Активен",
                table: "Клиенты",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "Дата последнего входа",
                table: "Клиенты",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Заблокирован до",
                table: "Клиенты",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Неудачные попытки входа",
                table: "Клиенты",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ФИО клиента",
                table: "Клиенты",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Код пользователя",
                table: "Refresh-токены",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClientEntityId",
                table: "Refresh-токены",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StafEntityTabNum",
                table: "Refresh-токены",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Refresh-токены_ClientEntityId",
                table: "Refresh-токены",
                column: "ClientEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Refresh-токены_StafEntityTabNum",
                table: "Refresh-токены",
                column: "StafEntityTabNum");

            migrationBuilder.AddForeignKey(
                name: "FK_Refresh-токены_Клиенты_ClientEntityId",
                table: "Refresh-токены",
                column: "ClientEntityId",
                principalTable: "Клиенты",
                principalColumn: "Код клиента");

            migrationBuilder.AddForeignKey(
                name: "FK_Refresh-токены_Сотрудники_StafEntityTabNum",
                table: "Refresh-токены",
                column: "StafEntityTabNum",
                principalTable: "Сотрудники",
                principalColumn: "Табельный номер");
        }
    }
}
