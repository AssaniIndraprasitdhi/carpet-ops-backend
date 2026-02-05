using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarpetOpsSystem.Models;

[Table("plans")]
public class Plan
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("cnv_id")]
    [MaxLength(50)]
    public string CnvId { get; set; } = string.Empty;

    [Column("roll_width_m")]
    public decimal RollWidthM { get; set; }

    [Column("outer_spacing")]
    public decimal OuterSpacing { get; set; }

    [Column("inner_spacing")]
    public decimal InnerSpacing { get; set; }

    [Column("used_length_m")]
    public decimal UsedLengthM { get; set; }

    [Column("used_area_sqm")]
    public decimal UsedAreaSqm { get; set; }

    [Column("total_area_sqm")]
    public decimal TotalAreaSqm { get; set; }

    [Column("waste_area_sqm")]
    public decimal WasteAreaSqm { get; set; }

    [Column("utilization_pct")]
    public decimal UtilizationPct { get; set; }

    [Column("piece_count")]
    public int PieceCount { get; set; }

    [Column("order_count")]
    public int OrderCount { get; set; }

    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = "Planned";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PlanOrder> PlanOrders { get; set; } = new List<PlanOrder>();
    public ICollection<PlanItem> PlanItems { get; set; } = new List<PlanItem>();
}
