using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusSheduleApp.Migrations
{
    /// <inheritdoc />
    public partial class AddBusRouteStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "BusRoutes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "BusRoutes");
        }
    }
}
