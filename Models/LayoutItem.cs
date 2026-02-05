using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarpetOpsSystem.Models;

[Table("layout_items")]
public class LayoutItem
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("layout_id")]
    public int LayoutId { get; set; }

    [Required]
    [Column("barcode_no")]
    [MaxLength(100)]
    public string BarcodeNo { get; set; } = string.Empty;

    [Column("x_position")]
    public decimal XPosition { get; set; }

    [Column("y_position")]
    public decimal YPosition { get; set; }

    [Column("width")]
    public decimal Width { get; set; }

    [Column("length")]
    public decimal Length { get; set; }

    [Column("is_rotated")]
    public bool IsRotated { get; set; }

    [Column("area_sqm")]
    public decimal AreaSqm { get; set; }

    [Required]
    [Column("order_no")]
    [MaxLength(50)]
    public string OrderNo { get; set; } = string.Empty;

    [Required]
    [Column("order_type")]
    [MaxLength(20)]
    public string OrderType { get; set; } = string.Empty;

    [Column("sequence")]
    public int Sequence { get; set; }

    public Layout? Layout { get; set; }
}
