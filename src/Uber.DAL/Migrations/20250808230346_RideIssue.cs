using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uber.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RideIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RideId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RideId",
                table: "Drivers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RideId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RideId",
                table: "Drivers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
