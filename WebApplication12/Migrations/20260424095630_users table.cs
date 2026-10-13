using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class userstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Id пользователя",
                table: "Сотрудники",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id пользователя",
                table: "Клиенты",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Пользователи",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Логин = table.Column<string>(type: "character varying(20)", maxLength: 100, nullable: false),
                    Пароль = table.Column<string>(type: "character varying(20)", maxLength: 255, nullable: false),
                    UserType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Активен = table.Column<bool>(type: "boolean", nullable: false),
                    Датапоследнеговхода = table.Column<DateTime>(name: "Дата последнего входа", type: "timestamp with time zone", nullable: true),
                    Неудачныепопыткивхода = table.Column<int>(name: "Неудачные попытки входа", type: "integer", nullable: false),
                    Заблокировандо = table.Column<DateTime>(name: "Заблокирован до", type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Пользователи", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Сотрудники_Id пользователя",
                table: "Сотрудники",
                column: "Id пользователя");

            migrationBuilder.CreateIndex(
                name: "IX_Клиенты_Id пользователя",
                table: "Клиенты",
                column: "Id пользователя");

            migrationBuilder.AddForeignKey(
                name: "FK_Клиенты_Пользователи_Id пользователя",
                table: "Клиенты",
                column: "Id пользователя",
                principalTable: "Пользователи",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Сотрудники_Пользователи_Id пользователя",
                table: "Сотрудники",
                column: "Id пользователя",
                principalTable: "Пользователи",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Клиенты_Пользователи_Id пользователя",
                table: "Клиенты");

            migrationBuilder.DropForeignKey(
                name: "FK_Сотрудники_Пользователи_Id пользователя",
                table: "Сотрудники");

            migrationBuilder.DropTable(
                name: "Пользователи");

            migrationBuilder.DropIndex(
                name: "IX_Сотрудники_Id пользователя",
                table: "Сотрудники");

            migrationBuilder.DropIndex(
                name: "IX_Клиенты_Id пользователя",
                table: "Клиенты");

            migrationBuilder.DropColumn(
                name: "Id пользователя",
                table: "Сотрудники");

            migrationBuilder.DropColumn(
                name: "Id пользователя",
                table: "Клиенты");
        }
    }
}
