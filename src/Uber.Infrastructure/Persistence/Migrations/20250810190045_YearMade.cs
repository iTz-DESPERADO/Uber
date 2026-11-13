using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uber.DAL.Migrations
{
    /// <inheritdoc />
    public partial class YearMade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Year",
                table: "Vehicles",
                newName: "YearMade");

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceld",
                table: "Rides",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCanceld",
                table: "Rides");

            migrationBuilder.RenameColumn(
                name: "YearMade",
                table: "Vehicles",
                newName: "Year");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
