using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LimLink_API.Migrations
{
    public partial class credit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Credit = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlanId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AmountPaid = table.Column<int>(type: "int", nullable: false),
                    CreditAdded = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentHistory", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountSetting");

            migrationBuilder.DropTable(
                name: "PaymentHistory");
        }
    }
}
