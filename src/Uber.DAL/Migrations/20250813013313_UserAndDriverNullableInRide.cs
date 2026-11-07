using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Uber.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UserAndDriverNullableInRide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Rides",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<int>(
                name: "Rate",
                table: "Rides",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "EndLat",
                table: "Rides",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "EndLng",
                table: "Rides",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StartLat",
                table: "Rides",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StartLng",
                table: "Rides",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Rides",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndLat",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "EndLng",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "StartLat",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "StartLng",
                table: "Rides");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rides");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Rides",
                newName: "Date");

            migrationBuilder.AlterColumn<int>(
                name: "Rate",
                table: "Rides",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
