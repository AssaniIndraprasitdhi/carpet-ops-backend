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
            .OrderByDescending(f => f.SyncedAt)
            .ToListAsync();
    }
}
