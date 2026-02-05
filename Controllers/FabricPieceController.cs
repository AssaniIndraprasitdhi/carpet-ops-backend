using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.Models;
using CarpetOpsSystem.Services;

namespace CarpetOpsSystem.Controllers;

[ApiController]
[Route("api/fabric-pieces")]
public class FabricPieceController : ControllerBase
{
    private readonly PostgresContext _context;
    private readonly AreaCalculationService _areaService;
    private readonly FabricPieceService _fabricPieceService;

    public FabricPieceController(
        PostgresContext context,
        AreaCalculationService areaService,
        FabricPieceService fabricPieceService)
    {
        _context = context;
        _areaService = areaService;
        _fabricPieceService = fabricPieceService;
    }

    [HttpGet]
    public async Task<ActionResult<List<FabricPiece>>> GetAll()
    {
        var pieces = await _fabricPieceService.GetAllFabricPiecesAsync();
        return Ok(pieces);
    }

    [HttpGet("{barcode}")]
    public async Task<ActionResult<FabricPiece>> GetByBarcode(string barcode)
    {
        var piece = await _context.FabricPieces
            .FirstOrDefaultAsync(f => f.BarcodeNo == barcode);

        if (piece == null)
        {
            return NotFound();
        }

        return Ok(piece);
    }

    [HttpGet("by-order/{orderNo}")]
    public async Task<ActionResult<List<FabricPiece>>> GetByOrderNo(string orderNo)
    {
        var pieces = await _context.FabricPieces
            .Where(f => f.OrderNo == orderNo)
            .OrderBy(f => f.ListNo)
            .ThenBy(f => f.ItemNo)
            .ToListAsync();

        return Ok(pieces);
    }

    [HttpGet("area-by-fabric-type")]
    public async Task<ActionResult<List<FabricTypeAreaSummary>>> GetAreaByFabricType()
    {
        var summary = await _areaService.GetAreaByFabricTypeAsync();
        return Ok(summary);
    }

    [HttpGet("utilization-stats")]
    public async Task<ActionResult<UtilizationStats>> GetUtilizationStats()
    {
        var stats = await _areaService.GetUtilizationStatsAsync();
        return Ok(stats);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<List<OrderSummary>>> GetOrders()
    {
        var orders = await _context.FabricPieces
            .GroupBy(f => new { f.OrderNo, f.OrderType })
            .Select(g => new OrderSummary
            {
                OrderNo = g.Key.OrderNo,
                OrderType = g.Key.OrderType,
                PieceCount = g.Count(),
                TotalAreaSqm = g.Sum(f => f.Sqm)
            })
            .OrderBy(o => o.OrderNo)
            .ToListAsync();

        return Ok(orders);
    }
}

public class OrderSummary
{
    public string OrderNo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public int PieceCount { get; set; }
    public decimal TotalAreaSqm { get; set; }
}
