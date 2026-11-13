using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uber.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddAtomicDriverReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rides_DriverId",
                table: "Rides");

            migrationBuilder.CreateIndex(
                name: "UX_Rides_ActiveDriver",
                table: "Rides",
                column: "DriverId",
                unique: true,
                filter: "[DriverId] IS NOT NULL AND [Status] IN (1, 4, 5)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_Rides_ActiveDriver",
                table: "Rides");

            migrationBuilder.CreateIndex(
                name: "IX_Rides_DriverId",
                table: "Rides",
                column: "DriverId");
        }
    }
}
