using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.Models;

namespace CarpetOpsSystem.Services;

public class FabricPieceService
{
    private readonly PostgresContext _context;

    public FabricPieceService(PostgresContext context)
    {
        _context = context;
    }

    public async Task<List<FabricPiece>> GetAllFabricPiecesAsync()
    {
        return await _context.FabricPieces
            .AsNoTracking()
            .OrderBy(f => f.OrderNo)
            .ThenBy(f => f.ListNo)
            .ThenBy(f => f.ItemNo)
            .ToListAsync();
    }

    public async Task<List<FabricPiece>> GetByOrderNoAsync(string orderNo)
    {
        orderNo = (orderNo ?? "").Trim();

        return await _context.FabricPieces
            .AsNoTracking()
            .Where(f => f.OrderNo == orderNo)
            .OrderBy(f => f.ListNo)
            .ThenBy(f => f.ItemNo)
            .ToListAsync();
    }

    public async Task<List<FabricPiece>> GetByCnvIdAsync(int cnvId)
    {
        var cnvIdStr = cnvId.ToString();

        return await _context.FabricPieces
            .AsNoTracking()
            .Where(f => f.CnvId == cnvIdStr)
            .OrderBy(f => f.OrderNo)
            .ThenBy(f => f.ListNo)
            .ThenBy(f => f.ItemNo)
            .ToListAsync();
    }

    public async Task<List<string>> GetOrderNosOnlyCnvIdAsync(int cnvId)
    {
        var cnvIdStr = cnvId.ToString();

        return await _context.FabricPieces
            .AsNoTracking()
            .Where(f => f.CnvId == cnvIdStr)
            .Select(f => f.OrderNo)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();
    }

    public async Task<List<OrderSummary>> GetOrderSummariesByCnvIdAsync(int cnvId)
    {
        var cnvIdStr = cnvId.ToString();

        return await _context.FabricPieces
            .AsNoTracking()
            .Where(f => f.CnvId == cnvIdStr)
            .GroupBy(f => new { f.OrderNo, f.OrderType })
            .Select(g => new OrderSummary
            {
                OrderNo = g.Key.OrderNo,
                OrderType = g.Key.OrderType,
                PieceCount = g.Count(),
                TotalAreaSqm = g.Sum(x => x.Sqm)
            })
            .OrderBy(x => x.OrderNo)
            .ToListAsync();
    }
}

public class OrderSummary
{
    public string OrderNo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public int PieceCount { get; set; }
    public decimal TotalAreaSqm { get; set; }
}
