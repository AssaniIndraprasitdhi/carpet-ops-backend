using Microsoft.AspNetCore.Mvc;
using CarpetOpsSystem.Services;

namespace CarpetOpsSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly DataSyncService _syncService;

    public SyncController(DataSyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost]
    public async Task<ActionResult<SyncResult>> SyncAll([FromQuery] DateTime? fromDate = null)
    {
        try
        {
            var result = await _syncService.SyncAllAsync(fromDate);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SyncResult
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpPost("by-orders")]
    public async Task<ActionResult<SyncResult>> SyncByOrders([FromBody] SyncByOrdersRequest request)
    {
        if (request.OrderNumbers == null || !request.OrderNumbers.Any())
        {
            return BadRequest("OrderNumbers is required");
        }

        try
        {
            var result = await _syncService.SyncByOrderNumbersAsync(request.OrderNumbers);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new SyncResult
            {
                Success = false,
                ErrorMessage = ex.Message
            });
        }
    }
}

public class SyncByOrdersRequest
{
    public List<string> OrderNumbers { get; set; } = new();
}
