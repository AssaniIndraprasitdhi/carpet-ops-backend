namespace CarpetOpsSystem.DTOs;

public class GenerateLayoutOptionsRequest
{
    public string? LayoutName { get; set; }
    public decimal TotalWidth { get; set; }
    public List<string> BarcodeNos { get; set; } = new();
}

public class LayoutOptionResponse
{
    public string Strategy { get; set; } = string.Empty;
    public decimal UtilizationPct { get; set; }
    public decimal UsedLengthM { get; set; }
    public decimal UsedAreaSqm { get; set; }
    public decimal WasteAreaSqm { get; set; }
    public decimal TotalAreaSqm { get; set; }
    public int PieceCount { get; set; }
    public List<PreviewItem> PreviewItems { get; set; } = new();
}

public class PreviewItem
{
    public string BarcodeNo { get; set; } = string.Empty;
    public string OrderNo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public decimal XPosition { get; set; }
    public decimal YPosition { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public bool IsRotated { get; set; }
}
