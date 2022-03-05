using Microsoft.EntityFrameworkCore.Migrations;

namespace LimLink_API.Migrations
{
    public partial class AddAccountTypeColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountType",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
              name: "AccountType",
              table: "AspNetUsers");
        }
    }
}
