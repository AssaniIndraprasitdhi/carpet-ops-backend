namespace CarpetOpsSystem.DTOs;

public class FabricTypeDto
{
    public int Id { get; set; }
    public string CnvId { get; set; } = string.Empty;
    public string? CnvDesc { get; set; }
    public decimal RollWidthM { get; set; }
}

public class OrderDto
{
    public string OrderNo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string CnvId { get; set; } = string.Empty;
    public string? CnvDesc { get; set; }
    public int PieceCount { get; set; }
    public decimal TotalAreaSqm { get; set; }
}

public class LayoutPreviewRequest
{
    public string CnvId { get; set; } = string.Empty;
    public List<string> OrderNos { get; set; } = new();
    public decimal OuterSpacing { get; set; } = 0.3m;
    public decimal InnerSpacing { get; set; } = 0.15m;
}

public class LayoutPreviewResponse
{
    public string CnvId { get; set; } = string.Empty;
    public decimal RollWidthM { get; set; }
    public decimal OuterSpacing { get; set; }
    public decimal InnerSpacing { get; set; }
    public decimal UsedLengthM { get; set; }
    public decimal UsedAreaSqm { get; set; }
    public decimal TotalAreaSqm { get; set; }
    public decimal WasteAreaSqm { get; set; }
    public decimal UtilizationPct { get; set; }
    public int PieceCount { get; set; }
    public List<LayoutPreviewItem> Items { get; set; } = new();
    public List<ExcludedPiece> Excluded { get; set; } = new();
}

public class LayoutPreviewItem
{
    public string BarcodeNo { get; set; } = string.Empty;
    public string OrderNo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public decimal AreaSqm { get; set; }
    public decimal XPosition { get; set; }
    public decimal YPosition { get; set; }
}

public class ExcludedPiece
{
    public string BarcodeNo { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class CreatePlanRequest
{
    public string CnvId { get; set; } = string.Empty;
    public List<string> OrderNos { get; set; } = new();
    public decimal RollWidthM { get; set; }
    public decimal OuterSpacing { get; set; }
    public decimal InnerSpacing { get; set; }
    public decimal UsedLengthM { get; set; }
    public decimal UsedAreaSqm { get; set; }
    public decimal TotalAreaSqm { get; set; }
    public decimal WasteAreaSqm { get; set; }
    public decimal UtilizationPct { get; set; }
    public int PieceCount { get; set; }
    public List<LayoutPreviewItem> Items { get; set; } = new();
}

public class PlanResponse
{
    public int Id { get; set; }
    public string CnvId { get; set; } = string.Empty;
    public decimal RollWidthM { get; set; }
    public decimal OuterSpacing { get; set; }
    public decimal InnerSpacing { get; set; }
    public decimal UsedLengthM { get; set; }
    public decimal UsedAreaSqm { get; set; }
    public decimal TotalAreaSqm { get; set; }
    public decimal WasteAreaSqm { get; set; }
    public decimal UtilizationPct { get; set; }
    public int PieceCount { get; set; }
    public int OrderCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<PlanOrderDto> Orders { get; set; } = new();
    public List<LayoutPreviewItem> Items { get; set; } = new();
}

public class PlanOrderDto
{
    public string OrderNo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public int PieceCount { get; set; }
    public decimal TotalAreaSqm { get; set; }
}

public class ConflictResponse
{
    public string Error { get; set; } = string.Empty;
    public List<string> LockedOrderNos { get; set; } = new();
}
