using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InvoiceManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class Attachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Attachments",
                newName: "UploadedAt");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Attachments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OriginalName",
                table: "Attachments",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "OriginalName",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "Attachments",
                newName: "FilePath");
        }
    }
}
