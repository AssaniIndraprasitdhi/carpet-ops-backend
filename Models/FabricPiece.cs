using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarpetOpsSystem.Models;

[Table("fabric_pieces")]
public class FabricPiece
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("barcode_no")]
    [MaxLength(100)]
    public string BarcodeNo { get; set; } = string.Empty;

    [Required]
    [Column("order_no")]
    [MaxLength(50)]
    public string OrderNo { get; set; } = string.Empty;

    [Column("list_no")]
    public int? ListNo { get; set; }

    [Column("item_no")]
    public int? ItemNo { get; set; }

    [Required]
    [Column("cnv_id")]
    [MaxLength(50)]
    public string CnvId { get; set; } = string.Empty;

    [Column("cnv_desc")]
    [MaxLength(255)]
    public string? CnvDesc { get; set; }

    [Column("as_plan")]
    [MaxLength(50)]
    public string? AsPlan { get; set; }

    [Column("width")]
    public decimal Width { get; set; }

    [Column("length")]
    public decimal Length { get; set; }

    [Column("sqm")]
    public decimal Sqm { get; set; }

    [Column("qty")]
    public int? Qty { get; set; }

    [Required]
    [Column("order_type")]
    [MaxLength(20)]
    public string OrderType { get; set; } = string.Empty;

    [Column("synced_at")]
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;

    [Column("layout_id")]
    public int? LayoutId { get; set; }

    public Layout? Layout { get; set; }
}
