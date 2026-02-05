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
            entity.HasIndex(e => e.CnvId).IsUnique();
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
    }
}
