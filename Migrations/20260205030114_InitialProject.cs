using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarpetOpsSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fabric_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cnv_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cnv_desc = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    roll_width = table.Column<decimal>(type: "numeric", nullable: false),
                    thickness = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fabric_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "layouts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    layout_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    total_width = table.Column<decimal>(type: "numeric", nullable: false),
                    total_length = table.Column<decimal>(type: "numeric", nullable: false),
                    total_area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    used_area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    waste_area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    waste_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    outer_spacing = table.Column<decimal>(type: "numeric", nullable: false),
                    inner_spacing = table.Column<decimal>(type: "numeric", nullable: false),
                    piece_count = table.Column<int>(type: "integer", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    sample_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layouts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fabric_pieces",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    barcode_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    list_no = table.Column<int>(type: "integer", nullable: true),
                    item_no = table.Column<int>(type: "integer", nullable: true),
                    cnv_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    cnv_desc = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    as_plan = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    width = table.Column<decimal>(type: "numeric", nullable: false),
                    length = table.Column<decimal>(type: "numeric", nullable: false),
                    sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    qty = table.Column<int>(type: "integer", nullable: true),
                    order_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    layout_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fabric_pieces", x => x.id);
                    table.ForeignKey(
                        name: "FK_fabric_pieces_layouts_layout_id",
                        column: x => x.layout_id,
                        principalTable: "layouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "layout_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    layout_id = table.Column<int>(type: "integer", nullable: false),
                    barcode_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    x_position = table.Column<decimal>(type: "numeric", nullable: false),
                    y_position = table.Column<decimal>(type: "numeric", nullable: false),
                    width = table.Column<decimal>(type: "numeric", nullable: false),
                    length = table.Column<decimal>(type: "numeric", nullable: false),
                    is_rotated = table.Column<bool>(type: "boolean", nullable: false),
                    area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layout_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_layout_items_layouts_layout_id",
                        column: x => x.layout_id,
                        principalTable: "layouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fabric_pieces_barcode_no",
                table: "fabric_pieces",
                column: "barcode_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fabric_pieces_cnv_id",
                table: "fabric_pieces",
                column: "cnv_id");

            migrationBuilder.CreateIndex(
                name: "IX_fabric_pieces_layout_id",
                table: "fabric_pieces",
                column: "layout_id");

            migrationBuilder.CreateIndex(
                name: "IX_fabric_pieces_order_no",
                table: "fabric_pieces",
                column: "order_no");

            migrationBuilder.CreateIndex(
                name: "IX_fabric_pieces_order_type",
                table: "fabric_pieces",
                column: "order_type");

            migrationBuilder.CreateIndex(
                name: "IX_fabric_types_cnv_id",
                table: "fabric_types",
                column: "cnv_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_layout_items_barcode_no",
                table: "layout_items",
                column: "barcode_no");

            migrationBuilder.CreateIndex(
                name: "IX_layout_items_layout_id",
                table: "layout_items",
                column: "layout_id");

            migrationBuilder.CreateIndex(
                name: "IX_layouts_created_at",
                table: "layouts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_layouts_status",
                table: "layouts",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fabric_pieces");

            migrationBuilder.DropTable(
                name: "fabric_types");

            migrationBuilder.DropTable(
                name: "layout_items");

            migrationBuilder.DropTable(
                name: "layouts");
        }
    }
}
