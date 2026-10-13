using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diploma.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatetableRepairactentity3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientVerificationData",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "DispatcherSignedAt",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "DispatcherSignedByTabNumber",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "DispatcherVerificationData",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "IsSignedByClient",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "IsSignedByDispatcher",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "IsVerifiedByDispatcher",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "SignatureMethod",
                table: "Акт выполненных работ");

            migrationBuilder.DropColumn(
                name: "SignedAt",
                table: "Акт выполненных работ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientVerificationData",
                table: "Акт выполненных работ",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DispatcherSignedAt",
                table: "Акт выполненных работ",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DispatcherSignedByTabNumber",
                table: "Акт выполненных работ",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DispatcherVerificationData",
                table: "Акт выполненных работ",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSignedByClient",
                table: "Акт выполненных работ",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSignedByDispatcher",
                table: "Акт выполненных работ",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerifiedByDispatcher",
                table: "Акт выполненных работ",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SignatureMethod",
                table: "Акт выполненных работ",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SignedAt",
                table: "Акт выполненных работ",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
