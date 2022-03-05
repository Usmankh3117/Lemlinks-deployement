using Microsoft.EntityFrameworkCore.Migrations;

namespace LimLink_API.Migrations
{
    public partial class SharedLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SharedBy",
                table: "HistoryLinks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SharedByName",
                table: "HistoryLinks",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharedBy",
                table: "HistoryLinks");

            migrationBuilder.DropColumn(
                name: "SharedByName",
                table: "HistoryLinks");
        }
    }
}
