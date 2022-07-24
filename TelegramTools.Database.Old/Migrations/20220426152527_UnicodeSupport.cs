using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramTools.Downloader.Migrations
{
    public partial class UnicodeSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentFiles",
                columns: table => new
                {
                    Hash = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Extension = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentFiles", x => x.Hash);
                });

            migrationBuilder.CreateTable(
                name: "DuplicateFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OrignalName = table.Column<string>(type: "TEXT", nullable: false),
                    DuplicateName = table.Column<string>(type: "TEXT", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuplicateFiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_Hash",
                table: "DocumentFiles",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentFiles_Name",
                table: "DocumentFiles",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateFiles_Hash",
                table: "DuplicateFiles",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_DuplicateFiles_Id",
                table: "DuplicateFiles",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentFiles");

            migrationBuilder.DropTable(
                name: "DuplicateFiles");
        }
    }
}
