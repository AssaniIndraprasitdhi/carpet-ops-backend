using Microsoft.AspNetCore.Mvc;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.DTOs;
using CarpetOpsSystem.Services;

namespace CarpetOpsSystem.Controllers;

[ApiController]
[Route("api/plans")]
public class PlanController : ControllerBase
{
    private readonly PlanService _planService;
    private readonly PostgresContext _context;

    public PlanController(PlanService planService, PostgresContext context)
    {
        _planService = planService;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<PlanResponse>>> GetAll([FromQuery] int? cnvId)
    {
        if (cnvId.HasValue)
        {
            var plans = await _planService.GetPlansByCnvIdAsync(cnvId.Value);
            return Ok(plans);
        }

        var allPlans = await _planService.GetAllPlansAsync();
        return Ok(allPlans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlanResponse>> GetById(int id)
    {
        var plans = await _planService.GetAllPlansAsync();
        var plan = plans.FirstOrDefault(p => p.Id == id);

        if (plan == null)
        {
            return NotFound();
        }

        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<PlanResponse>> Create([FromBody] CreatePlanRequest request)
    {
        if (request.CnvId <= 0)
        {
            return BadRequest(new { error = "CnvId is required" });
        }

        if (request.OrderNos == null || !request.OrderNos.Any())
        {
            return BadRequest(new { error = "OrderNos is required" });
        }

        try
        {
            var (plan, conflictOrders) = await _planService.CreatePlanAsync(request);

            if (conflictOrders != null && conflictOrders.Any())
            {
                return Conflict(new ConflictResponse
                {
                    Error = "Orders already planned",
                    LockedOrderNos = conflictOrders
                });
            }

            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var plan = await _context.Plans.FindAsync(id);

        if (plan == null)
        {
            return NotFound();
        }

        _context.Plans.Remove(plan);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
