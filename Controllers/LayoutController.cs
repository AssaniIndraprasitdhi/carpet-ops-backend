using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.DTOs;
using CarpetOpsSystem.Models;
using CarpetOpsSystem.Services;

namespace CarpetOpsSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LayoutController : ControllerBase
{
    private readonly LayoutCalculationService _layoutService;
    private readonly AreaCalculationService _areaService;
    private readonly PlanService _planService;
    private readonly PostgresContext _context;

    public LayoutController(
        LayoutCalculationService layoutService,
        AreaCalculationService areaService,
        PlanService planService,
        PostgresContext context)
    {
        _layoutService = layoutService;
        _areaService = areaService;
        _planService = planService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Layout>>> GetAll()
    {
        var layouts = await _layoutService.GetAllLayoutsAsync();
        return Ok(layouts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Layout>> GetById(int id)
    {
        var layout = await _context.Layouts
            .Include(l => l.Items.OrderBy(i => i.Sequence))
            .FirstOrDefaultAsync(l => l.Id == id);

        if (layout == null)
        {
            return NotFound();
        }

        return Ok(layout);
    }

    [HttpPost("calculate")]
    public async Task<ActionResult<Layout>> Calculate([FromBody] CalculateLayoutRequest request)
    {
        if (request.FabricWidth <= 0)
        {
            return BadRequest("FabricWidth must be greater than 0");
        }

        if (string.IsNullOrWhiteSpace(request.LayoutName))
        {
            return BadRequest("LayoutName is required");
        }

        try
        {
            var layout = await _layoutService.CalculateLayoutAsync(
                request.FabricWidth,
                request.LayoutName);

            return Ok(layout);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("calculate/by-barcodes")]
    public async Task<ActionResult<Layout>> CalculateByBarcodes([FromBody] CalculateByBarcodesRequest request)
    {
        if (request.Barcodes == null || !request.Barcodes.Any())
        {
            return BadRequest("Barcodes is required");
        }

        if (request.FabricWidth <= 0)
        {
            return BadRequest("FabricWidth must be greater than 0");
        }

        try
        {
            var layout = await _layoutService.CalculateLayoutByBarcodesAsync(
                request.Barcodes,
                request.FabricWidth,
                request.LayoutName ?? $"Layout-{DateTime.UtcNow:yyyyMMddHHmmss}");

            return Ok(layout);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("calculate/by-orders")]
    public async Task<ActionResult<Layout>> CalculateByOrders([FromBody] CalculateByOrdersRequest request)
    {
        if (request.OrderNumbers == null || !request.OrderNumbers.Any())
        {
            return BadRequest("OrderNumbers is required");
        }

        if (request.FabricWidth <= 0)
        {
            return BadRequest("FabricWidth must be greater than 0");
        }

        try
        {
            var layout = await _layoutService.CalculateLayoutByOrdersAsync(
                request.OrderNumbers,
                request.FabricWidth,
                request.LayoutName ?? $"Layout-{DateTime.UtcNow:yyyyMMddHHmmss}");

            return Ok(layout);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("options")]
    public async Task<ActionResult<List<LayoutOptionResponse>>> GenerateOptions([FromBody] GenerateLayoutOptionsRequest request)
    {
        if (request.BarcodeNos == null || !request.BarcodeNos.Any())
        {
            return BadRequest("BarcodeNos is required");
        }

        if (request.TotalWidth <= 0)
        {
            return BadRequest("TotalWidth must be greater than 0");
        }

        try
        {
            var options = await _layoutService.GenerateOptionsAsync(request);
            return Ok(options);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("preview")]
    public async Task<ActionResult<LayoutPreviewResponse>> Preview([FromBody] LayoutPreviewRequest request)
    {
        try
        {
            var preview = await _planService.GeneratePreviewAsync(request);
            return Ok(preview);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}/area-summary")]
    public async Task<ActionResult<AreaSummary>> GetAreaSummary(int id)
    {
        var summary = await _areaService.GetLayoutAreaSummaryAsync(id);

        if (!summary.Exists)
        {
            return NotFound();
        }

        return Ok(summary);
    }

    [HttpGet("{id}/area-by-order")]
    public async Task<ActionResult<List<OrderAreaSummary>>> GetAreaByOrder(int id)
    {
        var summary = await _areaService.GetAreaByOrderAsync(id);
        return Ok(summary);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var layout = await _context.Layouts.FindAsync(id);

        if (layout == null)
        {
            return NotFound();
        }

        var pieces = await _context.FabricPieces
            .Where(f => f.LayoutId == id)
            .ToListAsync();

        foreach (var piece in pieces)
        {
            piece.LayoutId = null;
        }

        _context.Layouts.Remove(layout);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CalculateLayoutRequest
{
    public decimal FabricWidth { get; set; }
    public string LayoutName { get; set; } = string.Empty;
}

public class CalculateByBarcodesRequest
{
    public List<string> Barcodes { get; set; } = new();
    public decimal FabricWidth { get; set; }
    public string? LayoutName { get; set; }
}

public class CalculateByOrdersRequest
{
    public List<string> OrderNumbers { get; set; } = new();
    public decimal FabricWidth { get; set; }
    public string? LayoutName { get; set; }
}
