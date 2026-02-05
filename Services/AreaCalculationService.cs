using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.Models;

namespace CarpetOpsSystem.Services;

public class AreaCalculationService
{
    private readonly PostgresContext _context;

    public AreaCalculationService(PostgresContext context)
    {
        _context = context;
    }

    public decimal CalculatePieceArea(decimal width, decimal length)
    {
        return width * length;
    }

    public async Task<AreaSummary> GetLayoutAreaSummaryAsync(int layoutId)
    {
        var layout = await _context.Layouts
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == layoutId);

        if (layout == null)
        {
            return new AreaSummary { LayoutId = layoutId, Exists = false };
        }

        return new AreaSummary
        {
            LayoutId = layoutId,
            Exists = true,
            LayoutName = layout.LayoutName,
            TotalAreaSqm = layout.TotalAreaSqm,
            UsedAreaSqm = layout.UsedAreaSqm,
            WasteAreaSqm = layout.WasteAreaSqm,
            WastePercentage = layout.WastePercentage,
            PieceCount = layout.PieceCount,
            OrderPiecesArea = layout.Items.Where(i => i.OrderType == "Order").Sum(i => i.AreaSqm),
            SamplePiecesArea = layout.Items.Where(i => i.OrderType == "Sample").Sum(i => i.AreaSqm)
        };
    }

    public async Task<List<OrderAreaSummary>> GetAreaByOrderAsync(int layoutId)
    {
        var items = await _context.LayoutItems
            .Where(i => i.LayoutId == layoutId)
            .GroupBy(i => new { i.OrderNo, i.OrderType })
            .Select(g => new OrderAreaSummary
            {
                OrderNo = g.Key.OrderNo,
                OrderType = g.Key.OrderType,
                PieceCount = g.Count(),
                TotalAreaSqm = g.Sum(i => i.AreaSqm)
            })
            .OrderBy(o => o.OrderNo)
            .ToListAsync();

        return items;
    }

    public async Task<List<FabricTypeAreaSummary>> GetAreaByFabricTypeAsync()
    {
        var summary = await _context.FabricPieces
            .GroupBy(f => new { f.CnvId, f.CnvDesc })
            .Select(g => new FabricTypeAreaSummary
            {
                CnvId = g.Key.CnvId,
                CnvDesc = g.Key.CnvDesc,
                PieceCount = g.Count(),
                TotalAreaSqm = g.Sum(f => f.Sqm),
                OrderPiecesCount = g.Count(f => f.OrderType == "Order"),
                SamplePiecesCount = g.Count(f => f.OrderType == "Sample")
            })
            .OrderBy(s => s.CnvId)
            .ToListAsync();

        return summary;
    }

    public async Task<UtilizationStats> GetUtilizationStatsAsync()
    {
        var layouts = await _context.Layouts
            .Where(l => l.Status == "Calculated")
            .ToListAsync();

        if (!layouts.Any())
        {
            return new UtilizationStats();
        }

        return new UtilizationStats
        {
            TotalLayouts = layouts.Count,
            TotalAreaUsedSqm = layouts.Sum(l => l.TotalAreaSqm),
            TotalFabricUsedSqm = layouts.Sum(l => l.UsedAreaSqm),
            TotalWasteSqm = layouts.Sum(l => l.WasteAreaSqm),
            AverageWastePercentage = layouts.Average(l => l.WastePercentage),
            BestWastePercentage = layouts.Min(l => l.WastePercentage),
            WorstWastePercentage = layouts.Max(l => l.WastePercentage),
            TotalPieces = layouts.Sum(l => l.PieceCount)
        };
    }
}

public class AreaSummary
{
    public int LayoutId { get; set; }
    public bool Exists { get; set; }
    public string? LayoutName { get; set; }
    public decimal TotalAreaSqm { get; set; }
    public decimal UsedAreaSqm { get; set; }
    public decimal WasteAreaSqm { get; set; }
    public decimal WastePercentage { get; set; }
    public int PieceCount { get; set; }
    public decimal OrderPiecesArea { get; set; }
    public decimal SamplePiecesArea { get; set; }
}

public class OrderAreaSummary
{
    public string OrderNo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public int PieceCount { get; set; }
    public decimal TotalAreaSqm { get; set; }
}

public class FabricTypeAreaSummary
{
    public string CnvId { get; set; } = string.Empty;
    public string? CnvDesc { get; set; }
    public int PieceCount { get; set; }
    public decimal TotalAreaSqm { get; set; }
    public int OrderPiecesCount { get; set; }
    public int SamplePiecesCount { get; set; }
}

public class UtilizationStats
{
    public int TotalLayouts { get; set; }
    public decimal TotalAreaUsedSqm { get; set; }
    public decimal TotalFabricUsedSqm { get; set; }
    public decimal TotalWasteSqm { get; set; }
    public decimal AverageWastePercentage { get; set; }
    public decimal BestWastePercentage { get; set; }
    public decimal WorstWastePercentage { get; set; }
    public int TotalPieces { get; set; }
}
