using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AOR.Migrations
{
    /// <inheritdoc />
    public partial class Link_Report_to_IdentityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_UserModel_UserId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "UserRoleModel");

            migrationBuilder.DropTable(
                name: "UserModel");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Reports",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_AspNetUsers_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_AspNetUsers_UserId",
                table: "Reports");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Reports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserModel",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrganizationOrgNr = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrgNr = table.Column<int>(type: "int", nullable: true),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModel", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserModel_Organizations_OrganizationOrgNr",
                        column: x => x.OrganizationOrgNr,
                        principalTable: "Organizations",
                        principalColumn: "OrgNr");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoleModel",
                columns: table => new
                {
                    UserRoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UserModelUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleModel", x => x.UserRoleId);
                    table.ForeignKey(
                        name: "FK_UserRoleModel_UserModel_UserModelUserId",
                        column: x => x.UserModelUserId,
                        principalTable: "UserModel",
                        principalColumn: "UserId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_UserModel_Email",
                table: "UserModel",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserModel_OrganizationOrgNr",
                table: "UserModel",
                column: "OrganizationOrgNr");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleModel_UserId_RoleId",
                table: "UserRoleModel",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleModel_UserModelUserId",
                table: "UserRoleModel",
                column: "UserModelUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_UserModel_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "UserModel",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
