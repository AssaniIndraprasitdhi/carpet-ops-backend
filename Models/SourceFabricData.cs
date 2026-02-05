namespace CarpetOpsSystem.Models;

public class SourceFabricData
{
    public string BarcodeNo { get; set; } = string.Empty;
    public string ORNO { get; set; } = string.Empty;
    public int ListNo { get; set; }
    public int ITEM_NO { get; set; }
    public string CnvID { get; set; } = string.Empty;
    public string? CnvDesc { get; set; }
    public string? ASPLAN { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public decimal Sqm { get; set; }
    public int Qty { get; set; }
    public string OrderType { get; set; } = string.Empty;

    public FabricPiece ToFabricPiece()
    {
        return new FabricPiece
        {
            BarcodeNo = BarcodeNo,
            OrderNo = ORNO,
            ListNo = ListNo,
            ItemNo = ITEM_NO,
            CnvId = CnvID ?? string.Empty,
            CnvDesc = CnvDesc,
            AsPlan = ASPLAN,
            Width = Width,
            Length = Length,
            Sqm = Sqm,
            Qty = Qty,
            OrderType = OrderType,
            SyncedAt = DateTime.UtcNow
        };
    }
}
