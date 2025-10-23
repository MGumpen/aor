using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AOR.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObstacleDatas",
                table: "ObstacleDatas");

            migrationBuilder.RenameTable(
                name: "ReportModel",
                newName: "Reports");

            migrationBuilder.RenameTable(
                name: "ObstacleDatas",
                newName: "ObstacleData");

            migrationBuilder.RenameIndex(
                name: "IX_ReportModel_UserId",
                table: "Reports",
                newName: "IX_Reports_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_ReportModel_Id",
                table: "Reports",
                newName: "IX_Reports_Id");

            migrationBuilder.AddColumn<int>(
                name: "PhotoModelPhotoId",
                table: "ObstacleData",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PositionModelPositionId",
                table: "ObstacleData",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reports",
                table: "Reports",
                column: "ReportId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObstacleData",
                table: "ObstacleData",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Photo = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.PhotoId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    PositionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Longitude = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.PositionId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ObstacleData_PhotoModelPhotoId",
                table: "ObstacleData",
                column: "PhotoModelPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_ObstacleData_PositionModelPositionId",
                table: "ObstacleData",
                column: "PositionModelPositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObstacleData_Photos_PhotoModelPhotoId",
                table: "ObstacleData",
                column: "PhotoModelPhotoId",
                principalTable: "Photos",
                principalColumn: "PhotoId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObstacleData_Positions_PositionModelPositionId",
                table: "ObstacleData",
                column: "PositionModelPositionId",
                principalTable: "Positions",
                principalColumn: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ObstacleData_Id",
                table: "Reports",
                column: "Id",
                principalTable: "ObstacleData",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObstacleData_Photos_PhotoModelPhotoId",
                table: "ObstacleData");

            migrationBuilder.DropForeignKey(
                name: "FK_ObstacleData_Positions_PositionModelPositionId",
                table: "ObstacleData");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ObstacleData_Id",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Users_UserId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Positions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reports",
                table: "Reports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ObstacleData",
                table: "ObstacleData");

            migrationBuilder.DropIndex(
                name: "IX_ObstacleData_PhotoModelPhotoId",
                table: "ObstacleData");

            migrationBuilder.DropIndex(
                name: "IX_ObstacleData_PositionModelPositionId",
                table: "ObstacleData");

            migrationBuilder.DropColumn(
                name: "PhotoModelPhotoId",
                table: "ObstacleData");

            migrationBuilder.DropColumn(
                name: "PositionModelPositionId",
                table: "ObstacleData");

            migrationBuilder.RenameTable(
                name: "Reports",
                newName: "ReportModel");

            migrationBuilder.RenameTable(
                name: "ObstacleData",
                newName: "ObstacleDatas");

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_ObstacleDatas",
                table: "ObstacleDatas",
                column: "Id");

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
    }
}
