using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LimLink_API.Migrations
{
    public partial class historylink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoryLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    IsShared = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryLinks", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryLinks");
        }
    }
}
