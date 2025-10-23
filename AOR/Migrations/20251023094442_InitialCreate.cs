using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AOR.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ObstacleDatas_Id",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "ReportModel");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_UserId",
                table: "ReportModel",
                newName: "IX_ReportModel_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Reports_Id",
                table: "ReportModel",
                newName: "IX_ReportModel_Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReportModel",
                table: "ReportModel",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportModel_ObstacleDatas_Id",
                table: "ReportModel",
                column: "Id",
                principalTable: "ObstacleDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReportModel_Users_UserId",
                table: "ReportModel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportModel_ObstacleDatas_Id",
                table: "ReportModel");

            migrationBuilder.DropForeignKey(
                name: "FK_ReportModel_Users_UserId",
                table: "ReportModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReportModel",
                table: "ReportModel");

            migrationBuilder.RenameTable(
                name: "ReportModel",
                newName: "Reports");

            migrationBuilder.RenameIndex(
                name: "IX_ReportModel_UserId",
                table: "Reports",
                newName: "IX_Reports_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportModel_Id",
                table: "Reports",
                newName: "IX_Reports_Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ObstacleDatas_Id",
                table: "Reports",
                column: "Id",
                principalTable: "ObstacleDatas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

