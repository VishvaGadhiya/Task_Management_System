using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class Init7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "51651ff3-801e-4bf9-8012-359b1ca43bf0", "AQAAAAIAAYagAAAAELDkATgi1afvswwZT7yFI2rR6/PVoFqPN9yI90mx76tAtMahAIEKmO28SebKpbfAHw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "90cbf855-f9bb-4430-b52f-37e435bbc6f4", "AQAAAAIAAYagAAAAEJoQNn9bdd7e2k0KXBMDSIz3/bIzLrmgP149lEyA4lz5LInkioQIU1kVD9yIoxEe8g==" });
        }
    }
}
