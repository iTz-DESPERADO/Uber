using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uber.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RideIssuev1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Drivers_DriverId1",
                table: "Rides");

            migrationBuilder.DropForeignKey(
                name: "FK_Rides_Users_UserId1",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_DriverId1",
                table: "Rides");

            migrationBuilder.DropIndex(
                name: "IX_Rides_UserId1",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "DriverId1",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Rides");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DriverId1",
                table: "Rides",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Rides",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rides_DriverId1",
                table: "Rides",
                column: "DriverId1");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_UserId1",
                table: "Rides",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Drivers_DriverId1",
                table: "Rides",
                column: "DriverId1",
                principalTable: "Drivers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rides_Users_UserId1",
                table: "Rides",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
