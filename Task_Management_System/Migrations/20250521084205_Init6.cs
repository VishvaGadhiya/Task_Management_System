using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class Init6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "90cbf855-f9bb-4430-b52f-37e435bbc6f4", "AQAAAAIAAYagAAAAEJoQNn9bdd7e2k0KXBMDSIz3/bIzLrmgP149lEyA4lz5LInkioQIU1kVD9yIoxEe8g==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "055b7251-c1e3-4a0b-857d-ace000b70dae", "AQAAAAIAAYagAAAAEC5BvkuTYxtRH9ZYUSMcEdKf0ggv7pxYgXPkhjed2tJwZzFhXDLqfsCLsR5sc+CfeA==" });
        }
    }
}
