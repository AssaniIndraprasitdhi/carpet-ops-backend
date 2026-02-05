using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Models;

namespace CarpetOpsSystem.Data;

public class PostgresContext : DbContext
{
    public PostgresContext(DbContextOptions<PostgresContext> options) : base(options)
    {
    }

    public DbSet<FabricPiece> FabricPieces { get; set; }
    public DbSet<FabricType> FabricTypes { get; set; }
    public DbSet<Layout> Layouts { get; set; }
    public DbSet<LayoutItem> LayoutItems { get; set; }
    public DbSet<Plan> Plans { get; set; }
    public DbSet<PlanOrder> PlanOrders { get; set; }
    public DbSet<PlanItem> PlanItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FabricPiece>(entity =>
        {
            entity.HasIndex(e => e.BarcodeNo).IsUnique();
            entity.HasIndex(e => e.OrderNo);
            entity.HasIndex(e => e.CnvId);
            entity.HasIndex(e => e.OrderType);
            entity.HasIndex(e => e.LayoutId);

            entity.HasOne(e => e.Layout)
                  .WithMany(l => l.FabricPieces)
                  .HasForeignKey(e => e.LayoutId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<FabricType>(entity =>
        {
            entity.HasIndex(e => e.ErpCode).IsUnique();
            entity.HasIndex(e => e.CnvId);
        });

        modelBuilder.Entity<Layout>(entity =>
        {
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<LayoutItem>(entity =>
        {
            entity.HasIndex(e => e.LayoutId);
            entity.HasIndex(e => e.BarcodeNo);

            entity.HasIndex(e => new { e.LayoutId, e.BarcodeNo })
                  .IsUnique()
                  .HasDatabaseName("ix_layout_items_layout_barcode_unique");

            entity.HasOne(e => e.Layout)
                  .WithMany(l => l.Items)
                  .HasForeignKey(e => e.LayoutId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasIndex(e => e.CnvId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<PlanOrder>(entity =>
        {
            entity.HasIndex(e => e.PlanId);
            entity.HasIndex(e => e.OrderNo);

            entity.HasIndex(e => new { e.CnvId, e.OrderNo })
                  .IsUnique()
                  .HasDatabaseName("ix_plan_orders_cnv_order_unique");

            entity.HasOne(e => e.Plan)
                  .WithMany(p => p.PlanOrders)
                  .HasForeignKey(e => e.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlanItem>(entity =>
        {
            entity.HasIndex(e => e.PlanId);
            entity.HasIndex(e => e.BarcodeNo);

            entity.HasIndex(e => new { e.PlanId, e.BarcodeNo })
                  .IsUnique()
                  .HasDatabaseName("ix_plan_items_plan_barcode_unique");

            entity.HasOne(e => e.Plan)
                  .WithMany(p => p.PlanItems)
                  .HasForeignKey(e => e.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
