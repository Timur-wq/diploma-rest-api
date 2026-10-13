using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Addremovecolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Дата подписания",
                table: "Договоры");

            migrationBuilder.DropColumn(
                name: "Комментарий",
                table: "Договоры");

            migrationBuilder.DropColumn(
                name: "Подписан клиентом",
                table: "Договоры");

            migrationBuilder.DropColumn(
                name: "Статус договора",
                table: "Договоры");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Дата подписания",
                table: "Договоры",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Комментарий",
                table: "Договоры",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Подписан клиентом",
                table: "Договоры",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Статус договора",
                table: "Договоры",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
