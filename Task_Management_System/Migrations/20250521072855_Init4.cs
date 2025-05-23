using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class Init4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "e317b8df-81d1-45a8-af20-e4155a44cc3a", "AQAAAAIAAYagAAAAEBiVqE58S6kKIBYURauS+aIYUiHQVHmnMtoASG3Jm1x22MlEOOnc4RDNCFOl/PMGJA==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "154b3977-46df-4966-919d-09a11e3bc737", "AQAAAAIAAYagAAAAEIiaXsM0epHnGFWQ0gRaI7rZdFRBdZ4c/f0CyI48GugRcWZqTpdMRMvms2wvxFbE/A==" });
        }
    }
}
