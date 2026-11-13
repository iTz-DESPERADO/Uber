using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Uber.DAL.DataBase;

#nullable disable

namespace Uber.DAL.Migrations;

[DbContext(typeof(UberDBContext))]
[Migration("20261109013000_AddApplicationUserCreatedAt")]
public sealed class AddApplicationUserCreatedAt : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "AspNetUsers",
            type: "datetime2",
            nullable: false,
            defaultValueSql: "SYSUTCDATETIME()");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "AspNetUsers");
    }
}
