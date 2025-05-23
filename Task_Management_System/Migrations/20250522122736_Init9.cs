using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class Init9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { 3, null, "Manager", "MANAGER" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "80b52b58-f7f0-4258-813f-f320ab871cae", "AQAAAAIAAYagAAAAEKr39J4Yv7mA9lmqXmy8jAWoqADTTnX+35LbyW3zDmqMRYych2RNTYUWElwyCZTXPQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "ec9d3d63-9205-4955-958e-5498b2551fff", "AQAAAAIAAYagAAAAEFoG0dST1Be9zjL4IIVPu8EiAF3skEhDU3U4EOQOG3b+RJ5DWft2UURbPQh9JhEbRA==" });
        }
    }
}
