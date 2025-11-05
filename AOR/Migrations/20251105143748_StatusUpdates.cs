using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AOR.Migrations
{
    /// <inheritdoc />
    public partial class StatusUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Obstacles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "StatusId",
                keyValue: 2,
                column: "Status",
                value: "Approved");

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "StatusId", "Status" },
                values: new object[,]
                {
                    { 4, "Draft" },
                    { 5, "Deleted" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "StatusId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Statuses",
                keyColumn: "StatusId",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Obstacles");

            migrationBuilder.UpdateData(
                table: "Statuses",
                keyColumn: "StatusId",
                keyValue: 2,
                column: "Status",
                value: "Accepted");
        }
    }
}
