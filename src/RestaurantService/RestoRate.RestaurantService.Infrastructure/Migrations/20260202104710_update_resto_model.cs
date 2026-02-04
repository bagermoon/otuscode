using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoRate.RestaurantService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class update_resto_model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "open_hours_close_time",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "open_hours_day_of_week",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "open_hours_is_closed",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "open_hours_open_time",
                table: "restaurants");

            migrationBuilder.AddColumn<string>(
                name: "open_hours",
                table: "restaurants",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "owner_id",
                table: "restaurants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "open_hours",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "owner_id",
                table: "restaurants");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "open_hours_close_time",
                table: "restaurants",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "open_hours_day_of_week",
                table: "restaurants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "open_hours_is_closed",
                table: "restaurants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "open_hours_open_time",
                table: "restaurants",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }
    }
}
