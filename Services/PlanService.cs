using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.DTOs;
using CarpetOpsSystem.Models;

namespace CarpetOpsSystem.Services;

public class PlanService
{
    private readonly PostgresContext _context;

    public PlanService(PostgresContext context)
    {
        _context = context;
    }

    public async Task<LayoutPreviewResponse> GeneratePreviewAsync(LayoutPreviewRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.CnvId))
        {
            throw new ArgumentException("CnvId is required");
        }

        var orderList = req.OrderNos?.Where(o => !string.IsNullOrWhiteSpace(o)).Distinct().ToList() ?? new List<string>();
        if (!orderList.Any())
        {
            throw new ArgumentException("OrderNos cannot be empty");
        }

        var fabricType = await _context.FabricTypes
            .FirstOrDefaultAsync(t => t.CnvId == req.CnvId);

        if (fabricType == null)
        {
            throw new KeyNotFoundException($"Fabric type not found: {req.CnvId}");
        }

        var allPieces = await _context.FabricPieces
            .Where(f => f.CnvId == req.CnvId && orderList.Contains(f.OrderNo))
            .ToListAsync();

        if (!allPieces.Any())
        {
            throw new ArgumentException("No pieces found for the specified orders");
        }

        var excluded = new List<ExcludedPiece>();
        var validPieces = new List<FabricPiece>();

        foreach (var piece in allPieces)
        {
            if (piece.Width <= 0 || piece.Length <= 0)
            {
                excluded.Add(new ExcludedPiece
                {
                    BarcodeNo = piece.BarcodeNo,
                    Reason = "Invalid dimensions (width or length <= 0)"
                });
            }
            else
            {
                validPieces.Add(piece);
            }
        }

        if (!validPieces.Any())
        {
            throw new ArgumentException("No valid pieces with positive dimensions");
        }

        var rollWidth = fabricType.RollWidth;
        var usableWidth = rollWidth - (2 * req.OuterSpacing);

        var heuristics = new List<(string Name, List<FabricPiece> Pieces)>
        {
            ("OriginalOrder", validPieces.ToList()),
            ("AreaDesc", validPieces.OrderByDescending(p => p.Width * p.Length).ToList()),
            ("MaxDimensionDesc", validPieces.OrderByDescending(p => Math.Max(p.Width, p.Length)).ToList())
        };

        LayoutPreviewResponse? bestResult = null;

        foreach (var (name, sortedPieces) in heuristics)
        {
            var (items, tooWide) = CalculatePositions(sortedPieces, usableWidth, req.OuterSpacing, req.InnerSpacing);

            decimal usedLengthM = req.OuterSpacing;
            if (items.Any())
            {
                usedLengthM = items.Max(i => i.YPosition + i.Length) + req.OuterSpacing;
            }

            decimal usedAreaSqm = items.Sum(i => i.AreaSqm);
            decimal totalAreaSqm = rollWidth * usedLengthM;
            decimal wasteAreaSqm = totalAreaSqm - usedAreaSqm;
            decimal utilizationPct = totalAreaSqm > 0 ? (usedAreaSqm / totalAreaSqm) * 100 : 0;

            var result = new LayoutPreviewResponse
            {
                CnvId = req.CnvId,
                RollWidthM = rollWidth,
                OuterSpacing = req.OuterSpacing,
                InnerSpacing = req.InnerSpacing,
                UsedLengthM = usedLengthM,
                UsedAreaSqm = usedAreaSqm,
                TotalAreaSqm = totalAreaSqm,
                WasteAreaSqm = wasteAreaSqm,
                UtilizationPct = utilizationPct,
                PieceCount = items.Count,
                Items = items,
                Excluded = excluded.Concat(tooWide.Select(b => new ExcludedPiece
                {
                    BarcodeNo = b,
                    Reason = "Width exceeds usable roll width"
                })).ToList()
            };

            if (bestResult == null || result.WasteAreaSqm < bestResult.WasteAreaSqm)
            {
                bestResult = result;
            }
        }

        return bestResult!;
    }

    public async Task<(PlanResponse Plan, List<string>? ConflictOrders)> CreatePlanAsync(CreatePlanRequest req)
    {
        var orderList = req.OrderNos?.Where(o => !string.IsNullOrWhiteSpace(o)).Distinct().ToList() ?? new List<string>();
        if (!orderList.Any())
        {
            throw new ArgumentException("OrderNos cannot be empty");
        }

        var existingPlanned = await _context.PlanOrders
            .Where(po => po.CnvId == req.CnvId && orderList.Contains(po.OrderNo))
            .Select(po => po.OrderNo)
            .ToListAsync();

        if (existingPlanned.Any())
        {
            return (null!, existingPlanned);
        }

        var pieces = await _context.FabricPieces
            .Where(f => f.CnvId == req.CnvId && orderList.Contains(f.OrderNo))
            .ToListAsync();

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var plan = new Plan
            {
                CnvId = req.CnvId,
                RollWidthM = req.RollWidthM,
                OuterSpacing = req.OuterSpacing,
                InnerSpacing = req.InnerSpacing,
                UsedLengthM = req.UsedLengthM,
                UsedAreaSqm = req.UsedAreaSqm,
                TotalAreaSqm = req.TotalAreaSqm,
                WasteAreaSqm = req.WasteAreaSqm,
                UtilizationPct = req.UtilizationPct,
                PieceCount = req.PieceCount,
                OrderCount = orderList.Count,
                Status = "Planned"
            };

            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var orderGroups = pieces
                .GroupBy(p => new { p.OrderNo, p.OrderType })
                .ToList();

            var planOrders = orderGroups.Select(g => new PlanOrder
            {
                PlanId = plan.Id,
                CnvId = req.CnvId,
                OrderNo = g.Key.OrderNo,
                OrderType = g.Key.OrderType,
                PieceCount = g.Count(),
                TotalAreaSqm = g.Sum(p => p.Sqm)
            }).ToList();

            _context.PlanOrders.AddRange(planOrders);

            int sequence = 1;
            var planItems = req.Items.Select(item => new PlanItem
            {
                PlanId = plan.Id,
                BarcodeNo = item.BarcodeNo,
                OrderNo = item.OrderNo,
                OrderType = item.OrderType,
                XPosition = item.XPosition,
                YPosition = item.YPosition,
                Width = item.Width,
                Length = item.Length,
                IsRotated = false,
                AreaSqm = item.AreaSqm,
                Sequence = sequence++
            }).ToList();

            _context.PlanItems.AddRange(planItems);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var response = new PlanResponse
            {
                Id = plan.Id,
                CnvId = plan.CnvId,
                RollWidthM = plan.RollWidthM,
                OuterSpacing = plan.OuterSpacing,
                InnerSpacing = plan.InnerSpacing,
                UsedLengthM = plan.UsedLengthM,
                UsedAreaSqm = plan.UsedAreaSqm,
                TotalAreaSqm = plan.TotalAreaSqm,
                WasteAreaSqm = plan.WasteAreaSqm,
                UtilizationPct = plan.UtilizationPct,
                PieceCount = plan.PieceCount,
                OrderCount = plan.OrderCount,
                Status = plan.Status,
                CreatedAt = plan.CreatedAt,
                Orders = planOrders.Select(po => new PlanOrderDto
                {
                    OrderNo = po.OrderNo,
                    OrderType = po.OrderType,
                    PieceCount = po.PieceCount,
                    TotalAreaSqm = po.TotalAreaSqm
                }).ToList(),
                Items = req.Items
            };

            return (response, null);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<PlanResponse>> GetPlansByCnvIdAsync(string cnvId)
    {
        var plans = await _context.Plans
            .Include(p => p.PlanOrders)
            .Include(p => p.PlanItems)
            .Where(p => p.CnvId == cnvId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return plans.Select(p => new PlanResponse
        {
            Id = p.Id,
            CnvId = p.CnvId,
            RollWidthM = p.RollWidthM,
            OuterSpacing = p.OuterSpacing,
            InnerSpacing = p.InnerSpacing,
            UsedLengthM = p.UsedLengthM,
            UsedAreaSqm = p.UsedAreaSqm,
            TotalAreaSqm = p.TotalAreaSqm,
            WasteAreaSqm = p.WasteAreaSqm,
            UtilizationPct = p.UtilizationPct,
            PieceCount = p.PieceCount,
            OrderCount = p.OrderCount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            Orders = p.PlanOrders.Select(po => new PlanOrderDto
            {
                OrderNo = po.OrderNo,
                OrderType = po.OrderType,
                PieceCount = po.PieceCount,
                TotalAreaSqm = po.TotalAreaSqm
            }).ToList(),
            Items = p.PlanItems.OrderBy(pi => pi.Sequence).Select(pi => new LayoutPreviewItem
            {
                BarcodeNo = pi.BarcodeNo,
                OrderNo = pi.OrderNo,
                OrderType = pi.OrderType,
                XPosition = pi.XPosition,
                YPosition = pi.YPosition,
                Width = pi.Width,
                Length = pi.Length,
                AreaSqm = pi.AreaSqm
            }).ToList()
        }).ToList();
    }

    public async Task<List<PlanResponse>> GetAllPlansAsync()
    {
        var plans = await _context.Plans
            .Include(p => p.PlanOrders)
            .Include(p => p.PlanItems)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return plans.Select(p => new PlanResponse
        {
            Id = p.Id,
            CnvId = p.CnvId,
            RollWidthM = p.RollWidthM,
            OuterSpacing = p.OuterSpacing,
            InnerSpacing = p.InnerSpacing,
            UsedLengthM = p.UsedLengthM,
            UsedAreaSqm = p.UsedAreaSqm,
            TotalAreaSqm = p.TotalAreaSqm,
            WasteAreaSqm = p.WasteAreaSqm,
            UtilizationPct = p.UtilizationPct,
            PieceCount = p.PieceCount,
            OrderCount = p.OrderCount,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            Orders = p.PlanOrders.Select(po => new PlanOrderDto
            {
                OrderNo = po.OrderNo,
                OrderType = po.OrderType,
                PieceCount = po.PieceCount,
                TotalAreaSqm = po.TotalAreaSqm
            }).ToList(),
            Items = p.PlanItems.OrderBy(pi => pi.Sequence).Select(pi => new LayoutPreviewItem
            {
                BarcodeNo = pi.BarcodeNo,
                OrderNo = pi.OrderNo,
                OrderType = pi.OrderType,
                XPosition = pi.XPosition,
                YPosition = pi.YPosition,
                Width = pi.Width,
                Length = pi.Length,
                AreaSqm = pi.AreaSqm
            }).ToList()
        }).ToList();
    }

    private (List<LayoutPreviewItem> Items, List<string> TooWide) CalculatePositions(
        List<FabricPiece> pieces,
        decimal usableWidth,
        decimal outerSpacing,
        decimal innerSpacing)
    {
        var items = new List<LayoutPreviewItem>();
        var tooWide = new List<string>();
        var rows = new List<RowState>();

        foreach (var piece in pieces)
        {
            var width = piece.Width;
            var length = piece.Length;

            if (width > usableWidth)
            {
                tooWide.Add(piece.BarcodeNo);
                continue;
            }

            RowState? targetRow = null;

            foreach (var row in rows)
            {
                var requiredSpace = width + (row.ItemCount > 0 ? innerSpacing : 0);
                if (row.RemainingWidth >= requiredSpace)
                {
                    targetRow = row;
                    break;
                }
            }

            if (targetRow == null)
            {
                var newRowY = outerSpacing;
                if (rows.Any())
                {
                    var lastRow = rows.Last();
                    newRowY = lastRow.YPosition + lastRow.Height + innerSpacing;
                }

                targetRow = new RowState
                {
                    YPosition = newRowY,
                    Height = 0,
                    RemainingWidth = usableWidth,
                    CurrentX = outerSpacing,
                    ItemCount = 0
                };
                rows.Add(targetRow);
            }

            var xPosition = targetRow.CurrentX;
            if (targetRow.ItemCount > 0)
                xPosition += innerSpacing;

            var item = new LayoutPreviewItem
            {
                BarcodeNo = piece.BarcodeNo,
                OrderNo = piece.OrderNo,
                OrderType = piece.OrderType,
                Width = width,
                Length = length,
                AreaSqm = width * length,
                XPosition = xPosition,
                YPosition = targetRow.YPosition
            };

            items.Add(item);
            targetRow.ItemCount++;
            targetRow.CurrentX = xPosition + width;
            targetRow.RemainingWidth = usableWidth - (targetRow.CurrentX - outerSpacing);
            if (length > targetRow.Height)
                targetRow.Height = length;
        }

        return (items, tooWide);
    }

    private class RowState
    {
        public decimal YPosition { get; set; }
        public decimal Height { get; set; }
        public decimal RemainingWidth { get; set; }
        public decimal CurrentX { get; set; }
        public int ItemCount { get; set; }
    }
}
