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
            .OrderBy(t => t.ErpCode)
            .Select(t => new FabricTypeDto
            {
                Id = t.Id,
                ErpCode = t.ErpCode,
                CnvId = t.CnvId,
                CnvDesc = t.CnvDesc,
                RollWidthM = t.RollWidthM,
                Thickness = t.Thickness
            })
            .ToListAsync();

        return Ok(types);
    }

    [HttpGet("{erpCode}")]
    public async Task<ActionResult<FabricTypeDto>> GetByErpCode(string erpCode)
    {
        var type = await _context.FabricTypes
            .Where(t => t.ErpCode == erpCode)
            .Select(t => new FabricTypeDto
            {
                Id = t.Id,
                ErpCode = t.ErpCode,
                CnvId = t.CnvId,
                CnvDesc = t.CnvDesc,
                RollWidthM = t.RollWidthM,
                Thickness = t.Thickness
            })
            .FirstOrDefaultAsync();

        if (type == null)
        {
            return NotFound();
        }

        return Ok(type);
    }
}
