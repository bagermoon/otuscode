using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RestoRate.Restaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingRestaurantFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "tag",
                table: "restaurants",
                newName: "open_hours_day_of_week");

            migrationBuilder.AlterColumn<Guid>(
                name: "id",
                table: "restaurants",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "address_full_address",
                table: "restaurants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "address_house",
                table: "restaurants",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "open_hours_close_time",
                table: "restaurants",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

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

            migrationBuilder.DropColumn(
                name: "address_full_address",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "address_house",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "open_hours_close_time",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "open_hours_is_closed",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "open_hours_open_time",
                table: "restaurants");

            migrationBuilder.RenameColumn(
                name: "open_hours_day_of_week",
                table: "restaurants",
                newName: "tag");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "restaurants",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
