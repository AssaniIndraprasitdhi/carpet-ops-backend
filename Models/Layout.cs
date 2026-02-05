using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarpetOpsSystem.Models;

[Table("layouts")]
public class Layout
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("layout_name")]
    [MaxLength(100)]
    public string LayoutName { get; set; } = string.Empty;

    [Column("total_width")]
    public decimal TotalWidth { get; set; }

    [Column("total_length")]
    public decimal TotalLength { get; set; }

    [Column("total_area_sqm")]
    public decimal TotalAreaSqm { get; set; }

    [Column("used_area_sqm")]
    public decimal UsedAreaSqm { get; set; }

    [Column("waste_area_sqm")]
    public decimal WasteAreaSqm { get; set; }

    [Column("waste_percentage")]
    public decimal WastePercentage { get; set; }

    [Column("outer_spacing")]
    public decimal OuterSpacing { get; set; }

    [Column("inner_spacing")]
    public decimal InnerSpacing { get; set; }

    [Column("piece_count")]
    public int PieceCount { get; set; }

    [Column("order_count")]
    public int OrderCount { get; set; }

    [Column("sample_count")]
    public int SampleCount { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("calculated_at")]
    public DateTime? CalculatedAt { get; set; }

    public ICollection<LayoutItem> Items { get; set; } = new List<LayoutItem>();
    public ICollection<FabricPiece> FabricPieces { get; set; } = new List<FabricPiece>();
}
