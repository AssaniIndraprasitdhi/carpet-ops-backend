using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.DTOs;

namespace CarpetOpsSystem.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly PostgresContext _context;

    public OrderController(PostgresContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetOrders([FromQuery] string? cnvId)
    {
        if (string.IsNullOrWhiteSpace(cnvId))
        {
            return BadRequest("cnvId is required");
        }

        var plannedOrders = await _context.PlanOrders
            .Where(po => po.CnvId == cnvId)
            .Select(po => po.OrderNo)
            .ToListAsync();

        var orders = await _context.FabricPieces
            .Where(f => f.CnvId == cnvId && !plannedOrders.Contains(f.OrderNo))
            .GroupBy(f => new { f.OrderNo, f.OrderType, f.CnvId, f.CnvDesc })
            .Select(g => new OrderDto
            {
                OrderNo = g.Key.OrderNo,
                OrderType = g.Key.OrderType,
                CnvId = g.Key.CnvId,
                CnvDesc = g.Key.CnvDesc,
                PieceCount = g.Count(),
                TotalAreaSqm = g.Sum(f => f.Sqm)
            })
            .OrderBy(o => o.OrderNo)
            .ToListAsync();

        return Ok(orders);
    }
}
