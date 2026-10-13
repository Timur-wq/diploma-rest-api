using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class smth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Исправляем тип колонки "Роль" с integer на varchar(50)
            migrationBuilder.AlterColumn<string>(
                name: "Роль",
                table: "Сотрудники",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            // ✅ Исправляем тип колонки "Пароль" с varchar(20) на varchar(256)
            migrationBuilder.AlterColumn<string>(
                name: "Пароль",
                table: "Сотрудники",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            // ✅ Добавляем значение по умолчанию для "Неудачные попытки входа"
            migrationBuilder.AlterColumn<int>(
                name: "Неудачные попытки входа",
                table: "Сотрудники",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            // ✅ Исправляем тип колонки "Логин" с varchar(20) на varchar(50)
            migrationBuilder.AlterColumn<string>(
                name: "Логин",
                table: "Сотрудники",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            // ✅ Добавляем значение по умолчанию для "Активен"
            migrationBuilder.AlterColumn<bool>(
                name: "Активен",
                table: "Сотрудники",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            // ✅ Создаём индексы (если не существуют)
            migrationBuilder.CreateIndex(
                name: "IX_Сотрудники_Логин",
                table: "Сотрудники",
                column: "Логин",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Сотрудники_Роль",
                table: "Сотрудники",
                column: "Роль");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ Удаляем индексы
            migrationBuilder.DropIndex(
                name: "IX_Сотрудники_Логин",
                table: "Сотрудники");

            migrationBuilder.DropIndex(
                name: "IX_Сотрудники_Роль",
                table: "Сотрудники");

            // ✅ Откат изменений
            migrationBuilder.AlterColumn<int>(
                name: "Роль",
                table: "Сотрудники",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Пароль",
                table: "Сотрудники",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<int>(
                name: "Неудачные попытки входа",
                table: "Сотрудники",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Логин",
                table: "Сотрудники",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "Активен",
                table: "Сотрудники",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);
        }
    }
}
