using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarpetOpsSystem.Models;

[Table("plan_orders")]
public class PlanOrder
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("plan_id")]
    public int PlanId { get; set; }

    [Required]
    [Column("cnv_id")]
    [MaxLength(50)]
    public string CnvId { get; set; } = string.Empty;

    [Required]
    [Column("order_no")]
    [MaxLength(50)]
    public string OrderNo { get; set; } = string.Empty;

    [Required]
    [Column("order_type")]
    [MaxLength(20)]
    public string OrderType { get; set; } = string.Empty;

    [Column("piece_count")]
    public int PieceCount { get; set; }

    [Column("total_area_sqm")]
    public decimal TotalAreaSqm { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Plan? Plan { get; set; }
}
