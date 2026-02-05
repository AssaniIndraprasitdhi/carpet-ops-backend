using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarpetOpsSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPlanTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cnv_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    roll_width_m = table.Column<decimal>(type: "numeric", nullable: false),
                    outer_spacing = table.Column<decimal>(type: "numeric", nullable: false),
                    inner_spacing = table.Column<decimal>(type: "numeric", nullable: false),
                    used_length_m = table.Column<decimal>(type: "numeric", nullable: false),
                    used_area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    total_area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    waste_area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    utilization_pct = table.Column<decimal>(type: "numeric", nullable: false),
                    piece_count = table.Column<int>(type: "integer", nullable: false),
                    order_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plan_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plan_id = table.Column<int>(type: "integer", nullable: false),
                    barcode_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    x_position = table.Column<decimal>(type: "numeric", nullable: false),
                    y_position = table.Column<decimal>(type: "numeric", nullable: false),
                    width = table.Column<decimal>(type: "numeric", nullable: false),
                    length = table.Column<decimal>(type: "numeric", nullable: false),
                    is_rotated = table.Column<bool>(type: "boolean", nullable: false),
                    area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_items_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plan_id = table.Column<int>(type: "integer", nullable: false),
                    cnv_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    piece_count = table.Column<int>(type: "integer", nullable: false),
                    total_area_sqm = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_orders", x => x.id);
                    table.ForeignKey(
                        name: "FK_plan_orders_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_layout_items_layout_barcode_unique",
                table: "layout_items",
                columns: new[] { "layout_id", "barcode_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_items_barcode_no",
                table: "plan_items",
                column: "barcode_no");

            migrationBuilder.CreateIndex(
                name: "ix_plan_items_plan_barcode_unique",
                table: "plan_items",
                columns: new[] { "plan_id", "barcode_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_items_plan_id",
                table: "plan_items",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "ix_plan_orders_cnv_order_unique",
                table: "plan_orders",
                columns: new[] { "cnv_id", "order_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_orders_order_no",
                table: "plan_orders",
                column: "order_no");

            migrationBuilder.CreateIndex(
                name: "IX_plan_orders_plan_id",
                table: "plan_orders",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_cnv_id",
                table: "plans",
                column: "cnv_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_created_at",
                table: "plans",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_plans_status",
                table: "plans",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plan_items");

            migrationBuilder.DropTable(
                name: "plan_orders");

            migrationBuilder.DropTable(
                name: "plans");

            migrationBuilder.DropIndex(
                name: "ix_layout_items_layout_barcode_unique",
                table: "layout_items");
        }
    }
}
