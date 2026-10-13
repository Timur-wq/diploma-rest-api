using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatetableRepairactentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Табельный номер диспетчера",
                table: "Акт выполненных работ",
                newName: "DispatcherSignedByTabNumber");

            migrationBuilder.RenameColumn(
                name: "Подписан клиентом",
                table: "Акт выполненных работ",
                newName: "IsSignedByClient");

            migrationBuilder.RenameColumn(
                name: "Подписан диспетчером",
                table: "Акт выполненных работ",
                newName: "IsSignedByDispatcher");

            migrationBuilder.RenameColumn(
                name: "Метод подписания",
                table: "Акт выполненных работ",
                newName: "SignatureMethod");

            migrationBuilder.RenameColumn(
                name: "Дата подписания диспетчером",
                table: "Акт выполненных работ",
                newName: "DispatcherSignedAt");

            migrationBuilder.RenameColumn(
                name: "Дата подписания",
                table: "Акт выполненных работ",
                newName: "SignedAt");

            migrationBuilder.RenameColumn(
                name: "Данные верификации клиента",
                table: "Акт выполненных работ",
                newName: "ClientVerificationData");

            migrationBuilder.RenameColumn(
                name: "Данные верификации диспетчера",
                table: "Акт выполненных работ",
                newName: "DispatcherVerificationData");

            migrationBuilder.RenameColumn(
                name: "Верифицирован диспетчером",
                table: "Акт выполненных работ",
                newName: "IsVerifiedByDispatcher");

            migrationBuilder.AddColumn<int>(
                name: "SignatureId",
                table: "Акт выполненных работ",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Акт выполненных работ_SignatureId",
                table: "Акт выполненных работ",
                column: "SignatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Акт выполненных работ_Подписи_SignatureId",
                table: "Акт выполненных работ",
                column: "SignatureId",
                principalTable: "Подписи",
                principalColumn: "Id подписей");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Акт выполненных работ_Подписи_SignatureId",
                table: "Акт выполненных работ");

            migrationBuilder.DropIndex(
                name: "IX_Акт выполненных работ_SignatureId",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "SignatureId",
                table: "Акт выполненных работ");

            migrationBuilder.RenameColumn(
                name: "SignedAt",
                table: "Акт выполненных работ",
                newName: "Дата подписания");

            migrationBuilder.RenameColumn(
                name: "SignatureMethod",
                table: "Акт выполненных работ",
                newName: "Метод подписания");

            migrationBuilder.RenameColumn(
                name: "IsVerifiedByDispatcher",
                table: "Акт выполненных работ",
                newName: "Верифицирован диспетчером");

            migrationBuilder.RenameColumn(
                name: "IsSignedByDispatcher",
                table: "Акт выполненных работ",
                newName: "Подписан диспетчером");

            migrationBuilder.RenameColumn(
                name: "IsSignedByClient",
                table: "Акт выполненных работ",
                newName: "Подписан клиентом");

            migrationBuilder.RenameColumn(
                name: "DispatcherVerificationData",
                table: "Акт выполненных работ",
                newName: "Данные верификации диспетчера");

            migrationBuilder.RenameColumn(
                name: "DispatcherSignedByTabNumber",
                table: "Акт выполненных работ",
                newName: "Табельный номер диспетчера");

            migrationBuilder.RenameColumn(
                name: "DispatcherSignedAt",
                table: "Акт выполненных работ",
                newName: "Дата подписания диспетчером");

            migrationBuilder.RenameColumn(
                name: "ClientVerificationData",
                table: "Акт выполненных работ",
                newName: "Данные верификации клиента");
        }
    }
}
