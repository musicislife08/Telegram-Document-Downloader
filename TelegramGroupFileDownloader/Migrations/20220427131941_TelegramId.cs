using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramGroupFileDownloader.Migrations
{
    public partial class TelegramId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramId",
                table: "DuplicateFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TelegramId",
                table: "DocumentFiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramId",
                table: "DuplicateFiles");

            migrationBuilder.DropColumn(
                name: "TelegramId",
                table: "DocumentFiles");
        }
    }
}
