using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarpetOpsSystem.Models;

[Table("fabric_types")]
public class FabricType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("erp_code")]
    [MaxLength(50)]
    public string ErpCode { get; set; } = string.Empty;

    [Column("cnv_id")]
    public int? CnvId { get; set; }

    [Column("cnv_desc")]
    [MaxLength(255)]
    public string? CnvDesc { get; set; }

    [Column("roll_width_m")]
    public decimal RollWidthM { get; set; }

    [Column("thickness")]
    public decimal Thickness { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
