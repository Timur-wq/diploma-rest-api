using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Addsignatureentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "SignatureId",
                table: "Договоры",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Подписи",
                columns: table => new
                {
                    Idподписей = table.Column<int>(name: "Id подписей", type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Подписанклиентом = table.Column<bool>(name: "Подписан клиентом", type: "boolean", nullable: false),
                    Датаподписания = table.Column<DateTime>(name: "Дата подписания", type: "timestamp with time zone", nullable: true),
                    Методподписания = table.Column<string>(name: "Метод подписания", type: "text", nullable: true),
                    Данныеверификацииклиента = table.Column<string>(name: "Данные верификации клиента", type: "text", nullable: true),
                    Подписандиспетчером = table.Column<bool>(name: "Подписан диспетчером", type: "boolean", nullable: false),
                    Датаподписаниядиспетчером = table.Column<DateTime>(name: "Дата подписания диспетчером", type: "timestamp with time zone", nullable: true),
                    Табельныйномердиспетчера = table.Column<int>(name: "Табельный номер диспетчера", type: "integer", nullable: true),
                    Данныеверификациидиспетчера = table.Column<string>(name: "Данные верификации диспетчера", type: "text", nullable: true),
                    Верифицировандиспетчером = table.Column<bool>(name: "Верифицирован диспетчером", type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Подписи", x => x.Idподписей);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Договоры_SignatureId",
                table: "Договоры",
                column: "SignatureId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Договоры_Подписи_SignatureId",
                table: "Договоры",
                column: "SignatureId",
                principalTable: "Подписи",
                principalColumn: "Id подписей");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Договоры_Подписи_SignatureId",
                table: "Договоры");

            migrationBuilder.DropForeignKey(
                name: "FK_Сотрудники_Пользователи_Id пользователя",
                table: "Сотрудники");

            migrationBuilder.DropTable(
                name: "Подписи");

            migrationBuilder.DropIndex(
                name: "IX_Договоры_SignatureId",
                table: "Договоры");

            migrationBuilder.DropColumn(
                name: "SignatureId",
                table: "Договоры");

            migrationBuilder.AlterColumn<int>(
                name: "Роль",
                table: "Сотрудники",
                type: "integer",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_Сотрудники_Пользователи_Id пользователя",
                table: "Сотрудники",
                column: "Id пользователя",
                principalTable: "Пользователи",
                principalColumn: "Id");
        }
    }
}
