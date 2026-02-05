using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.DTOs;

namespace CarpetOpsSystem.Controllers;

[ApiController]
[Route("api/fabric-types")]
public class FabricTypeController : ControllerBase
{
    private readonly PostgresContext _context;

    public FabricTypeController(PostgresContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<FabricTypeDto>>> GetAll()
    {
        var types = await _context.FabricTypes
            .OrderBy(t => t.CnvId)
            .Select(t => new FabricTypeDto
            {
                Id = t.Id,
                CnvId = t.CnvId,
                CnvDesc = t.CnvDesc,
                RollWidthM = t.RollWidth
            })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("{cnvId}")]
    public async Task<ActionResult<FabricTypeDto>> GetByCnvId(string cnvId)
    {
        var type = await _context.FabricTypes
            .Where(t => t.CnvId == cnvId)
            .Select(t => new FabricTypeDto
            {
                Id = t.Id,
                CnvId = t.CnvId,
                CnvDesc = t.CnvDesc,
                RollWidthM = t.RollWidth
            })
            .FirstOrDefaultAsync();

        if (type == null)
        {
            return NotFound();
        }

        return Ok(type);
    }
}
