using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Updatetablesmth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Данные верификации диспетчера",
                table: "Акт выполненных работ",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Данные верификации клиента",
                table: "Акт выполненных работ",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Дата подписания диспетчером",
                table: "Акт выполненных работ",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Подписан диспетчером",
                table: "Акт выполненных работ",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Табельный номер диспетчера",
                table: "Акт выполненных работ",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Данные верификации диспетчера",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "Данные верификации клиента",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "Дата подписания диспетчером",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "Подписан диспетчером",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "Табельный номер диспетчера",
                table: "Акт выполненных работ");
        }
    }
}
