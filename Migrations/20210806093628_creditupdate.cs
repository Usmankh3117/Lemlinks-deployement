using Microsoft.EntityFrameworkCore.Migrations;

namespace LimLink_API.Migrations
{
    public partial class creditupdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Credit",
                table: "AccountSetting",
                newName: "TotalCredit");

            migrationBuilder.AddColumn<long>(
                name: "AvailableCredit",
                table: "AccountSetting",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableCredit",
                table: "AccountSetting");

            migrationBuilder.RenameColumn(
                name: "TotalCredit",
                table: "AccountSetting",
                newName: "Credit");
        }
    }
}
