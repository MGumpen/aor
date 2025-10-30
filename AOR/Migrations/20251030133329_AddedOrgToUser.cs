using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AOR.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrgToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrgNr",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrgNr",
                table: "AspNetUsers",
                column: "OrgNr");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Organizations_OrgNr",
                table: "AspNetUsers",
                column: "OrgNr",
                principalTable: "Organizations",
                principalColumn: "OrgNr");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Organizations_OrgNr",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_OrgNr",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrgNr",
                table: "AspNetUsers");
        }
    }
}
