using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RestoRate.Restaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "restaurants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    phone_number_operator_code = table.Column<string>(type: "text", nullable: false),
                    phone_number_number = table.Column<string>(type: "text", nullable: false),
                    phone_number_extension = table.Column<string>(type: "text", nullable: true),
                    email_address = table.Column<string>(type: "text", nullable: false),
                    location_latitude = table.Column<double>(type: "double precision", nullable: false),
                    location_longitude = table.Column<double>(type: "double precision", nullable: false),
                    average_check_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    average_check_currency = table.Column<string>(type: "text", nullable: false),
                    tag = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_restaurants", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "restaurants");
        }
    }
}
