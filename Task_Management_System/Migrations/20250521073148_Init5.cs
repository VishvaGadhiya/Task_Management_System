using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class Init5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "055b7251-c1e3-4a0b-857d-ace000b70dae", "AQAAAAIAAYagAAAAEC5BvkuTYxtRH9ZYUSMcEdKf0ggv7pxYgXPkhjed2tJwZzFhXDLqfsCLsR5sc+CfeA==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "e317b8df-81d1-45a8-af20-e4155a44cc3a", "AQAAAAIAAYagAAAAEBiVqE58S6kKIBYURauS+aIYUiHQVHmnMtoASG3Jm1x22MlEOOnc4RDNCFOl/PMGJA==" });
        }
    }
}
