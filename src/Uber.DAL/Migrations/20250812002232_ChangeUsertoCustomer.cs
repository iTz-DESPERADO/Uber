using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uber.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUsertoCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "User_TotalRatings",
                table: "AspNetUsers",
                newName: "Driver_TotalRatings");

            migrationBuilder.RenameColumn(
                name: "User_TotalRatingPoints",
                table: "AspNetUsers",
                newName: "Driver_TotalRatingPoints");

            migrationBuilder.RenameColumn(
                name: "User_ModifiedAt",
                table: "AspNetUsers",
                newName: "Driver_ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "User_Balance",
                table: "AspNetUsers",
                newName: "Driver_Balance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Driver_TotalRatings",
                table: "AspNetUsers",
                newName: "User_TotalRatings");

            migrationBuilder.RenameColumn(
                name: "Driver_TotalRatingPoints",
                table: "AspNetUsers",
                newName: "User_TotalRatingPoints");

            migrationBuilder.RenameColumn(
                name: "Driver_ModifiedAt",
                table: "AspNetUsers",
                newName: "User_ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "Driver_Balance",
                table: "AspNetUsers",
                newName: "User_Balance");
        }
    }
}
