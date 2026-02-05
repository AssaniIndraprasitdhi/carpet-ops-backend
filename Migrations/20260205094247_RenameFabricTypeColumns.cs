using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarpetOpsSystem.Migrations
{
    public partial class RenameFabricTypeColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_fabric_types_cnv_id",
                table: "fabric_types");

            migrationBuilder.RenameColumn(
                name: "cnv_id",
                table: "fabric_types",
                newName: "erp_code");

            migrationBuilder.RenameColumn(
                name: "cnv_id_ref",
                table: "fabric_types",
                newName: "cnv_id");

            migrationBuilder.RenameColumn(
                name: "roll_width",
                table: "fabric_types",
                newName: "roll_width_m");

            migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM pg_class c
                    JOIN pg_namespace n ON n.oid = c.relnamespace
                    WHERE c.relkind = 'i'
                    AND n.nspname = 'public'
                    AND c.relname = 'IX_fabric_types_cnv_id_ref'
                ) THEN
                    EXECUTE 'ALTER INDEX "IX_fabric_types_cnv_id_ref" RENAME TO "IX_fabric_types_cnv_id"';
                END IF;
            END $$;
            """);

            migrationBuilder.CreateIndex(
                name: "IX_fabric_types_cnv_id",
                table: "fabric_types",
                column: "cnv_id");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_fabric_types_erp_code",
                table: "fabric_types");

            migrationBuilder.RenameIndex(
                name: "IX_fabric_types_cnv_id",
                table: "fabric_types",
                newName: "IX_fabric_types_cnv_id_ref");

            migrationBuilder.RenameColumn(
                name: "roll_width_m",
                table: "fabric_types",
                newName: "roll_width");

            migrationBuilder.RenameColumn(
                name: "cnv_id",
                table: "fabric_types",
                newName: "cnv_id_ref");

            migrationBuilder.RenameColumn(
                name: "erp_code",
                table: "fabric_types",
                newName: "cnv_id");

            migrationBuilder.CreateIndex(
                name: "IX_fabric_types_cnv_id",
                table: "fabric_types",
                column: "cnv_id",
                unique: true);
        }
    }
}
