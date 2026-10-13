using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class smthnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Только изменяем колонки, НЕ трогаем первичный ключ!

            // Переименовать колонку "Модель СВТ" → "Модель"
            migrationBuilder.RenameColumn(
                name: "Модель СВТ",
                table: "Заявки на ТО СВТ",
                newName: "Модель");

            // Изменить тип колонки "Статус" с integer на varchar(20)
            migrationBuilder.AlterColumn<string>(
                name: "Статус",
                table: "Заявки на ТО СВТ",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Откат изменений
            migrationBuilder.RenameColumn(
                name: "Модель",
                table: "Заявки на ТО СВТ",
                newName: "Модель СВТ");

            migrationBuilder.AlterColumn<int>(
                name: "Статус",
                table: "Заявки на ТО СВТ",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
