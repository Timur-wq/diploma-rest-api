using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeRefreshTokensPolymorphic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_Staf",
                table: "Refresh-токены");



            migrationBuilder.RenameColumn(
                name: "Код сотрудника",
                table: "Refresh-токены",
                newName: "Код пользователя");

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

            migrationBuilder.AddColumn<string>(
                name: "Логин",
                table: "Клиенты",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Неудачные попытки входа",
                table: "Клиенты",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Пароль",
                table: "Клиенты",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<string>(
                name: "Тип пользователя",
                table: "Refresh-токены",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Дата последнего входа",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Заблокирован до",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Логин",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Неудачные попытки входа",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Пароль",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "ClientEntityId",
                table: "Refresh-токены");

            migrationBuilder.DropColumn(
                name: "StafEntityTabNum",
                table: "Refresh-токены");

            migrationBuilder.DropColumn(
                name: "Тип пользователя",
                table: "Refresh-токены");

            migrationBuilder.RenameColumn(
                name: "Код пользователя",
                table: "Refresh-токены",
                newName: "Код сотрудника");

            migrationBuilder.CreateIndex(
                name: "IX_Refresh-токены_Код сотрудника",
                table: "Refresh-токены",
                column: "Код сотрудника");

            migrationBuilder.AddForeignKey(
                name: "FK_Refresh-токены_Сотрудники_Код сотрудника",
                table: "Refresh-токены",
                column: "Код сотрудника",
                principalTable: "Сотрудники",
                principalColumn: "Табельный номер",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
