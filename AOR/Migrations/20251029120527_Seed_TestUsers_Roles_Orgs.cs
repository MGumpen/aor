using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AOR.Migrations
{
    /// <inheritdoc />
    public partial class Seed_TestUsers_Roles_Orgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "OrgNr",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "OrgNr",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "OrgNr",
                keyValue: 3);

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "Roles",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "OrgNr", "OrgName" },
                values: new object[,]
                {
                    { 123456789, "Norsk Luftambulanse" },
                    { 234567891, "Luftforsvaret" },
                    { 345678912, "Politiets helikoptertjeneste" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "OrgNr",
                keyValue: 123456789);

            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "OrgNr",
                keyValue: 234567891);

            migrationBuilder.DeleteData(
                table: "Organizations",
                keyColumn: "OrgNr",
                keyValue: 345678912);

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "Roles",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Organizations",
                columns: new[] { "OrgNr", "OrgName" },
                values: new object[,]
                {
                    { 1, "Norsk Luftambulanse" },
                    { 2, "Luftforsvaret" },
                    { 3, "Politiets helikoptertjeneste" }
                });
        }
    }
}
