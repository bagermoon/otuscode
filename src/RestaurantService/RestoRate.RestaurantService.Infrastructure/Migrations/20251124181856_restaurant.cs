using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoRate.RestaurantService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class restaurant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "restaurants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    phone_number_operator_code = table.Column<string>(type: "text", nullable: false),
                    phone_number_number = table.Column<string>(type: "text", nullable: false),
                    phone_number_extension = table.Column<string>(type: "text", nullable: true),
                    email_address = table.Column<string>(type: "text", nullable: false),
                    address_full_address = table.Column<string>(type: "text", nullable: false),
                    address_house = table.Column<string>(type: "text", nullable: false),
                    location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    location_longitude = table.Column<double>(type: "double precision", nullable: false),
                    open_hours_day_of_week = table.Column<int>(type: "integer", nullable: false),
                    open_hours_open_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    open_hours_close_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    open_hours_is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    average_check_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    average_check_currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_cuisinetype",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    cuisine_type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_cuisinetype", x => x.id);
                    table.ForeignKey(
                        name: "fk_restaurant_cuisinetype_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    alt_text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_primary = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_restaurant_images_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "restaurant_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurant_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_restaurant_tags_restaurants_restaurant_id",
                        column: x => x.restaurant_id,
                        principalTable: "restaurants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_cuisinetype_restaurant_id",
                table: "restaurant_cuisinetype",
                column: "restaurant_id");

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_images_restaurant_id_is_primary",
                table: "restaurant_images",
                columns: new[] { "restaurant_id", "is_primary" });

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_tags_restaurant_id",
                table: "restaurant_tags",
                column: "restaurant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "restaurant_cuisinetype");

            migrationBuilder.DropTable(
                name: "restaurant_images");

            migrationBuilder.DropTable(
                name: "restaurant_tags");

            migrationBuilder.DropTable(
                name: "restaurants");
        }
    }
}
