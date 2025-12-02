using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AOR.Migrations
{
    /// <inheritdoc />
    public partial class Added_Report_Deleted_Voltage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "Voltage",
                table: "Obstacles");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.StatusId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "StatusId", "Status" },
                values: new object[,]
                {
                    { 1, "Pending" },
                    { 2, "Approved" },
                    { 3, "Rejected" },
                    { 4, "Draft" },
                    { 5, "Deleted" }

                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StatusId",
                table: "Reports",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Statuses_StatusId",
                table: "Reports",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Statuses_StatusId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropIndex(
                name: "IX_Reports_StatusId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Reports",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Reports",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<double>(
                name: "Voltage",
                table: "Obstacles",
                type: "double",
                nullable: true);
        }
    }
}
