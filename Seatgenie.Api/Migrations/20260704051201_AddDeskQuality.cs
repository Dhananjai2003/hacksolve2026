using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Seatgenie.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeskQuality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "desk_quality",
                columns: table => new
                {
                    quality_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    quality_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_desk_quality", x => x.quality_id);
                });

            migrationBuilder.CreateTable(
                name: "desk_quality_mapping",
                columns: table => new
                {
                    mapping_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    desk_id = table.Column<string>(type: "text", nullable: false),
                    quality_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_desk_quality_mapping", x => x.mapping_id);
                    table.ForeignKey(
                        name: "f_k_desk_quality_mapping_desk_desk_id",
                        column: x => x.desk_id,
                        principalTable: "desk",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_desk_quality_mapping_desk_quality_quality_id",
                        column: x => x.quality_id,
                        principalTable: "desk_quality",
                        principalColumn: "quality_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_desk_quality_mapping_desk_id_quality_id",
                table: "desk_quality_mapping",
                columns: new[] { "desk_id", "quality_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_desk_quality_mapping_quality_id",
                table: "desk_quality_mapping",
                column: "quality_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "desk_quality_mapping");

            migrationBuilder.DropTable(
                name: "desk_quality");
        }
    }
}
