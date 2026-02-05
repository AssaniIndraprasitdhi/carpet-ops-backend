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
    public async Task<ActionResult<List<OrderDto>>> GetOrders([FromQuery] int? cnvId)
    {
        if (!cnvId.HasValue)
        {
            return BadRequest("cnvId is required");
        }

        var cnvIdStr = cnvId.Value.ToString();

        var plannedOrderNos = await _context.PlanOrders
            .Where(po => po.CnvId == cnvId.Value)
            .Select(po => po.OrderNo)
            .ToListAsync();

        var orders = await _context.FabricPieces
            .Where(f => f.CnvId == cnvIdStr && !plannedOrderNos.Contains(f.OrderNo))
            .GroupBy(f => new { f.OrderNo, f.OrderType })
            .Select(g => new OrderDto
            {
                OrderNo = g.Key.OrderNo,
                OrderType = g.Key.OrderType,
                CnvId = cnvId.Value,
                PieceCount = g.Count(),
                TotalAreaSqm = g.Sum(f => f.Sqm)
            })
            .OrderBy(o => o.OrderNo)
            .ToListAsync();

        return Ok(orders);
    }
}
