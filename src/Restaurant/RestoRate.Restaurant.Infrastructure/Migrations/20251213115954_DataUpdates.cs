using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestoRate.Restaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DataUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_restaurant_tags_restaurant_id",
                table: "restaurant_tags");

            migrationBuilder.DropColumn(
                name: "tag",
                table: "restaurant_tags");

            migrationBuilder.AddColumn<int>(
                name: "restaurant_status",
                table: "restaurants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "tag_id",
                table: "restaurant_tags",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    normalized_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tag", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_tags_restaurant_id_tag_id",
                table: "restaurant_tags",
                columns: new[] { "restaurant_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_tags_tag_id",
                table: "restaurant_tags",
                column: "tag_id");

            migrationBuilder.AddForeignKey(
                name: "fk_restaurant_tags_tag_tag_id",
                table: "restaurant_tags",
                column: "tag_id",
                principalTable: "tag",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_restaurant_tags_tag_tag_id",
                table: "restaurant_tags");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropIndex(
                name: "ix_restaurant_tags_restaurant_id_tag_id",
                table: "restaurant_tags");

            migrationBuilder.DropIndex(
                name: "ix_restaurant_tags_tag_id",
                table: "restaurant_tags");

            migrationBuilder.DropColumn(
                name: "restaurant_status",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "tag_id",
                table: "restaurant_tags");

            migrationBuilder.AddColumn<int>(
                name: "tag",
                table: "restaurant_tags",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_restaurant_tags_restaurant_id",
                table: "restaurant_tags",
                column: "restaurant_id");
        }
    }
}
