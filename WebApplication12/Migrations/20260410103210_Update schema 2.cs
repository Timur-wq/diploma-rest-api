using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class Updateschema2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_ЗИП у поставщиков_SupplierSpareEntityId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_Поставщики_SupplierId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_Учёт ЗИП_SpareCode",
                table: "Заявки на ЗИП");

            migrationBuilder.DropIndex(
                name: "IX_Заявки на ЗИП_SpareCode",
                table: "Заявки на ЗИП");

            migrationBuilder.DropIndex(
                name: "IX_Заявки на ЗИП_SupplierId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropIndex(
                name: "IX_Заявки на ЗИП_SupplierSpareEntityId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "SpareCode",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "SupplierSpareEntityId",
                table: "Заявки на ЗИП");

            migrationBuilder.RenameColumn(
                name: "Код ЗИП-поставщика",
                table: "Заявки на ЗИП",
                newName: "Код ЗИП у поставщика");

            migrationBuilder.AddColumn<DateTime>(
                name: "Дата создания",
                table: "Заявки на ЗИП",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Комментарий",
                table: "Заявки на ЗИП",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_Код ЗИП у поставщика",
                table: "Заявки на ЗИП",
                column: "Код ЗИП у поставщика");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_ЗИП у поставщиков_Код ЗИП у поставщика",
                table: "Заявки на ЗИП",
                column: "Код ЗИП у поставщика",
                principalTable: "ЗИП у поставщиков",
                principalColumn: "Код ЗИП-Поставщик",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Заявки на ЗИП_ЗИП у поставщиков_Код ЗИП у поставщика",
                table: "Заявки на ЗИП");

            migrationBuilder.DropIndex(
                name: "IX_Заявки на ЗИП_Код ЗИП у поставщика",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "Дата создания",
                table: "Заявки на ЗИП");

            migrationBuilder.DropColumn(
                name: "Комментарий",
                table: "Заявки на ЗИП");

            migrationBuilder.RenameColumn(
                name: "Код ЗИП у поставщика",
                table: "Заявки на ЗИП",
                newName: "Код ЗИП-поставщика");

            migrationBuilder.AddColumn<int>(
                name: "SpareCode",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierSpareEntityId",
                table: "Заявки на ЗИП",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_SpareCode",
                table: "Заявки на ЗИП",
                column: "SpareCode");

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_SupplierId",
                table: "Заявки на ЗИП",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Заявки на ЗИП_SupplierSpareEntityId",
                table: "Заявки на ЗИП",
                column: "SupplierSpareEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_ЗИП у поставщиков_SupplierSpareEntityId",
                table: "Заявки на ЗИП",
                column: "SupplierSpareEntityId",
                principalTable: "ЗИП у поставщиков",
                principalColumn: "Код ЗИП-Поставщик");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_Поставщики_SupplierId",
                table: "Заявки на ЗИП",
                column: "SupplierId",
                principalTable: "Поставщики",
                principalColumn: "Код поставщика");

            migrationBuilder.AddForeignKey(
                name: "FK_Заявки на ЗИП_Учёт ЗИП_SpareCode",
                table: "Заявки на ЗИП",
                column: "SpareCode",
                principalTable: "Учёт ЗИП",
                principalColumn: "Код ЗИП");
        }
    }
}
