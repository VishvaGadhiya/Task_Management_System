using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class Init8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "ec9d3d63-9205-4955-958e-5498b2551fff", "AQAAAAIAAYagAAAAEFoG0dST1Be9zjL4IIVPu8EiAF3skEhDU3U4EOQOG3b+RJ5DWft2UURbPQh9JhEbRA==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "51651ff3-801e-4bf9-8012-359b1ca43bf0", "AQAAAAIAAYagAAAAELDkATgi1afvswwZT7yFI2rR6/PVoFqPN9yI90mx76tAtMahAIEKmO28SebKpbfAHw==" });
        }
    }
}
