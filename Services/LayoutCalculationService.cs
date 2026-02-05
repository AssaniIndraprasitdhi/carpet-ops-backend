using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Config;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.DTOs;
using CarpetOpsSystem.Models;

namespace CarpetOpsSystem.Services;

public class LayoutCalculationService
{
    private readonly PostgresContext _context;
    private readonly AppSettings _settings;

    public LayoutCalculationService(PostgresContext context, AppSettings settings)
    {
        _context = context;
        _settings = settings;
    }

    public async Task<List<Layout>> GetAllLayoutsAsync()
    {
        return await _context.Layouts
            .Include(l => l.Items.OrderBy(i => i.Sequence))
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<Layout> CalculateLayoutAsync(decimal fabricWidth, string layoutName)
    {
        var pieces = await _context.FabricPieces
            .Where(f => f.LayoutId == null)
            .OrderByDescending(f => f.Length)
            .ThenByDescending(f => f.Width)
            .ToListAsync();

        return await CalculateLayoutForPiecesAsync(pieces, fabricWidth, layoutName);
    }

    public async Task<Layout> CalculateLayoutByBarcodesAsync(
        IEnumerable<string> barcodes,
        decimal fabricWidth,
        string layoutName)
    {
        var barcodeList = barcodes
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Distinct()
            .ToList();

        var pieces = await _context.FabricPieces
            .Where(f => barcodeList.Contains(f.BarcodeNo))
            .OrderByDescending(f => f.Length)
            .ThenByDescending(f => f.Width)
            .ToListAsync();

        return await CalculateLayoutForPiecesAsync(pieces, fabricWidth, layoutName);
    }

    public async Task<Layout> CalculateLayoutByOrdersAsync(
        IEnumerable<string> orderNumbers,
        decimal fabricWidth,
        string layoutName)
    {
        var orderList = orderNumbers.ToList();
        var pieces = await _context.FabricPieces
            .Where(f => orderList.Contains(f.OrderNo))
            .OrderByDescending(f => f.Length)
            .ThenByDescending(f => f.Width)
            .ToListAsync();

        return await CalculateLayoutForPiecesAsync(pieces, fabricWidth, layoutName);
    }

    public async Task<List<LayoutOptionResponse>> GenerateOptionsAsync(GenerateLayoutOptionsRequest req)
    {
        var barcodeList = req.BarcodeNos
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Distinct()
            .ToList();

        if (!barcodeList.Any())
        {
            throw new InvalidOperationException("BarcodeNos cannot be empty");
        }

        var pieces = await _context.FabricPieces
            .Where(f => barcodeList.Contains(f.BarcodeNo))
            .ToListAsync();

        var notFoundBarcodes = barcodeList.Except(pieces.Select(p => p.BarcodeNo)).ToList();
        if (notFoundBarcodes.Any())
        {
            throw new InvalidOperationException(
                $"Barcodes not found: {string.Join(", ", notFoundBarcodes)}");
        }

        var invalidPieces = pieces.Where(p => p.Width <= 0 || p.Length <= 0).ToList();
        var validPieces = pieces.Where(p => p.Width > 0 && p.Length > 0).ToList();

        if (!validPieces.Any())
        {
            throw new InvalidOperationException("No valid pieces with positive width and length");
        }

        var strategies = new[] { "Standard", "SizeBased", "Rotated", "CutCorner" };
        var results = new List<LayoutOptionResponse>();

        foreach (var strategy in strategies)
        {
            var sortedPieces = SortPiecesForStrategy(validPieces, strategy);
            var result = CreateLayoutOptionForStrategy(sortedPieces, req.TotalWidth, strategy);
            results.Add(result);
        }

        return results.OrderByDescending(r => r.UtilizationPct).ToList();
    }

    private List<FabricPiece> SortPiecesForStrategy(List<FabricPiece> pieces, string strategy)
    {
        return strategy switch
        {
            "Standard" => pieces.OrderByDescending(p => p.Length).ThenByDescending(p => p.Width).ToList(),
            "SizeBased" => pieces.OrderByDescending(p => p.Width * p.Length).ToList(),
            "Rotated" => pieces.OrderByDescending(p => p.Length).ThenByDescending(p => p.Width).ToList(),
            "CutCorner" => pieces.OrderByDescending(p => p.Width * p.Length).ToList(),
            _ => pieces.ToList()
        };
    }

    private LayoutOptionResponse CreateLayoutOptionForStrategy(
        List<FabricPiece> pieces,
        decimal fabricWidth,
        string strategy)
    {
        var outerSpacing = _settings.OuterSpacing;
        var innerSpacing = _settings.InnerSpacing;
        var usableWidth = fabricWidth - (2 * outerSpacing);

        var layoutItems = strategy == "Rotated"
            ? CalculatePositionsWithRotationStateless(pieces, usableWidth, outerSpacing, innerSpacing)
            : CalculatePositionsStateless(pieces, usableWidth, outerSpacing, innerSpacing);

        decimal totalLength = outerSpacing;
        if (layoutItems.Any())
        {
            totalLength = layoutItems.Max(i => i.YPosition + i.Length) + outerSpacing;
        }

        decimal usedAreaSqm = pieces.Sum(p => p.Width * p.Length);
        decimal totalAreaSqm = fabricWidth * totalLength;
        decimal wasteAreaSqm = totalAreaSqm - usedAreaSqm;
        decimal utilizationPct = totalAreaSqm > 0 ? (usedAreaSqm / totalAreaSqm) * 100 : 0;

        return new LayoutOptionResponse
        {
            Strategy = strategy,
            UtilizationPct = utilizationPct,
            UsedLengthM = totalLength,
            UsedAreaSqm = usedAreaSqm,
            WasteAreaSqm = wasteAreaSqm,
            TotalAreaSqm = totalAreaSqm,
            PieceCount = layoutItems.Count,
            PreviewItems = layoutItems.Select(i => new PreviewItem
            {
                BarcodeNo = i.BarcodeNo,
                OrderNo = i.OrderNo,
                OrderType = i.OrderType,
                XPosition = i.XPosition,
                YPosition = i.YPosition,
                Width = i.Width,
                Length = i.Length,
                IsRotated = i.IsRotated
            }).ToList()
        };
    }

    private List<PlacedItem> CalculatePositionsStateless(
        List<FabricPiece> pieces,
        decimal usableWidth,
        decimal outerSpacing,
        decimal innerSpacing)
    {
        var items = new List<PlacedItem>();
        var rows = new List<LayoutRowStateless>();

        foreach (var piece in pieces)
        {
            var width = piece.Width;
            var length = piece.Length;
            bool rotated = false;

            if (width > usableWidth && length <= usableWidth)
            {
                (width, length) = (length, width);
                rotated = true;
            }

            if (width > usableWidth)
                continue;

            LayoutRowStateless? targetRow = null;
            foreach (var row in rows)
            {
                if (row.RemainingWidth >= width + (row.Items.Any() ? innerSpacing : 0))
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

                targetRow = new LayoutRowStateless
                {
                    YPosition = newRowY,
                    Height = 0,
                    RemainingWidth = usableWidth,
                    CurrentX = outerSpacing
                };
                rows.Add(targetRow);
            }

            var xPosition = targetRow.CurrentX;
            if (targetRow.Items.Any())
                xPosition += innerSpacing;

            var item = new PlacedItem
            {
                BarcodeNo = piece.BarcodeNo,
                OrderNo = piece.OrderNo,
                OrderType = piece.OrderType,
                XPosition = xPosition,
                YPosition = targetRow.YPosition,
                Width = width,
                Length = length,
                IsRotated = rotated
            };

            items.Add(item);
            targetRow.Items.Add(item);
            targetRow.CurrentX = xPosition + width;
            targetRow.RemainingWidth = usableWidth - (targetRow.CurrentX - outerSpacing);
            if (length > targetRow.Height)
                targetRow.Height = length;
        }

        return items;
    }

    private List<PlacedItem> CalculatePositionsWithRotationStateless(
        List<FabricPiece> pieces,
        decimal usableWidth,
        decimal outerSpacing,
        decimal innerSpacing)
    {
        var items = new List<PlacedItem>();
        var rows = new List<LayoutRowStateless>();

        foreach (var piece in pieces)
        {
            var width = piece.Width;
            var length = piece.Length;
            bool rotated = false;

            if (width > usableWidth && length <= usableWidth)
            {
                (width, length) = (length, width);
                rotated = true;
            }

            if (width > usableWidth)
                continue;

            LayoutRowStateless? targetRow = null;
            bool needsRotationForFit = false;

            foreach (var row in rows)
            {
                var requiredSpace = width + (row.Items.Any() ? innerSpacing : 0);
                if (row.RemainingWidth >= requiredSpace)
                {
                    targetRow = row;
                    break;
                }

                if (!rotated && length <= usableWidth)
                {
                    var rotatedWidth = length;
                    var rotatedRequiredSpace = rotatedWidth + (row.Items.Any() ? innerSpacing : 0);
                    if (row.RemainingWidth >= rotatedRequiredSpace)
                    {
                        targetRow = row;
                        needsRotationForFit = true;
                        break;
                    }
                }
            }

            if (needsRotationForFit)
            {
                (width, length) = (length, width);
                rotated = true;
            }

            if (targetRow == null)
            {
                var newRowY = outerSpacing;
                if (rows.Any())
                {
                    var lastRow = rows.Last();
                    newRowY = lastRow.YPosition + lastRow.Height + innerSpacing;
                }

                targetRow = new LayoutRowStateless
                {
                    YPosition = newRowY,
                    Height = 0,
                    RemainingWidth = usableWidth,
                    CurrentX = outerSpacing
                };
                rows.Add(targetRow);
            }

            var xPosition = targetRow.CurrentX;
            if (targetRow.Items.Any())
                xPosition += innerSpacing;

            var item = new PlacedItem
            {
                BarcodeNo = piece.BarcodeNo,
                OrderNo = piece.OrderNo,
                OrderType = piece.OrderType,
                XPosition = xPosition,
                YPosition = targetRow.YPosition,
                Width = width,
                Length = length,
                IsRotated = rotated
            };

            items.Add(item);
            targetRow.Items.Add(item);
            targetRow.CurrentX = xPosition + width;
            targetRow.RemainingWidth = usableWidth - (targetRow.CurrentX - outerSpacing);
            if (length > targetRow.Height)
                targetRow.Height = length;
        }

        return items;
    }

    private List<LayoutItem> CalculatePositionsWithRotation(
        List<FabricPiece> pieces,
        decimal usableWidth,
        decimal outerSpacing,
        decimal innerSpacing,
        int layoutId)
    {
        var layoutItems = new List<LayoutItem>();
        var rows = new List<LayoutRow>();
        int sequence = 1;

        foreach (var piece in pieces)
        {
            var width = piece.Width;
            var length = piece.Length;
            var rotated = false;

            if (width > usableWidth && length <= usableWidth)
            {
                (width, length) = (length, width);
                rotated = true;
            }

            if (width > usableWidth)
            {
                continue;
            }

            LayoutRow? targetRow = null;
            bool needsRotationForFit = false;

            foreach (var row in rows)
            {
                var requiredSpace = width + (row.Items.Any() ? innerSpacing : 0);
                if (row.RemainingWidth >= requiredSpace)
                {
                    targetRow = row;
                    break;
                }

                if (!rotated && length <= usableWidth)
                {
                    var rotatedWidth = length;
                    var rotatedRequiredSpace = rotatedWidth + (row.Items.Any() ? innerSpacing : 0);
                    if (row.RemainingWidth >= rotatedRequiredSpace)
                    {
                        targetRow = row;
                        needsRotationForFit = true;
                        break;
                    }
                }
            }

            if (needsRotationForFit)
            {
                (width, length) = (length, width);
                rotated = true;
            }

            if (targetRow == null)
            {
                var newRowY = outerSpacing;
                if (rows.Any())
                {
                    var lastRow = rows.Last();
                    newRowY = lastRow.YPosition + lastRow.Height + innerSpacing;
                }

                targetRow = new LayoutRow
                {
                    YPosition = newRowY,
                    Height = 0,
                    RemainingWidth = usableWidth,
                    CurrentX = outerSpacing
                };
                rows.Add(targetRow);
            }

            var xPosition = targetRow.CurrentX;
            if (targetRow.Items.Any())
            {
                xPosition += innerSpacing;
            }

            var layoutItem = new LayoutItem
            {
                LayoutId = layoutId,
                BarcodeNo = piece.BarcodeNo,
                XPosition = xPosition,
                YPosition = targetRow.YPosition,
                Width = width,
                Length = length,
                IsRotated = rotated,
                AreaSqm = piece.Sqm,
                OrderNo = piece.OrderNo,
                OrderType = piece.OrderType,
                Sequence = sequence++
            };

            layoutItems.Add(layoutItem);
            targetRow.Items.Add(layoutItem);

            targetRow.CurrentX = xPosition + width;
            targetRow.RemainingWidth = usableWidth - (targetRow.CurrentX - outerSpacing);
            if (length > targetRow.Height)
            {
                targetRow.Height = length;
            }
        }

        return layoutItems;
    }

    private async Task<Layout> CalculateLayoutForPiecesAsync(
        List<FabricPiece> pieces,
        decimal fabricWidth,
        string layoutName)
    {
        var duplicateBarcodes = pieces
            .GroupBy(p => p.BarcodeNo)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateBarcodes.Any())
        {
            throw new InvalidOperationException(
                $"Duplicate barcodes not allowed in layout: {string.Join(", ", duplicateBarcodes)}");
        }

        var outerSpacing = _settings.OuterSpacing;
        var innerSpacing = _settings.InnerSpacing;

        var usableWidth = fabricWidth - (2 * outerSpacing);

        var layout = new Layout
        {
            LayoutName = layoutName,
            TotalWidth = fabricWidth,
            OuterSpacing = outerSpacing,
            InnerSpacing = innerSpacing,
            Status = "Pending"
        };

        _context.Layouts.Add(layout);
        await _context.SaveChangesAsync();

        var layoutItems = CalculatePositions(pieces, usableWidth, outerSpacing, innerSpacing, layout.Id);

        _context.LayoutItems.AddRange(layoutItems);

        decimal totalLength = outerSpacing;
        if (layoutItems.Any())
        {
            totalLength = layoutItems.Max(i => i.YPosition + i.Length) + outerSpacing;
        }

        layout.TotalLength = totalLength;
        layout.TotalAreaSqm = fabricWidth * totalLength;
        layout.UsedAreaSqm = pieces.Sum(p => p.Sqm);
        layout.WasteAreaSqm = layout.TotalAreaSqm - layout.UsedAreaSqm;
        layout.WastePercentage = layout.TotalAreaSqm > 0
            ? (layout.WasteAreaSqm / layout.TotalAreaSqm) * 100
            : 0;
        layout.PieceCount = pieces.Count;
        layout.OrderCount = pieces.Count(p => p.OrderType == "Order");
        layout.SampleCount = pieces.Count(p => p.OrderType == "Sample");
        layout.Status = "Calculated";
        layout.CalculatedAt = DateTime.UtcNow;

        foreach (var piece in pieces)
        {
            piece.LayoutId = layout.Id;
        }

        await _context.SaveChangesAsync();

        return layout;
    }

    private List<LayoutItem> CalculatePositions(
        List<FabricPiece> pieces,
        decimal usableWidth,
        decimal outerSpacing,
        decimal innerSpacing,
        int layoutId)
    {
        var layoutItems = new List<LayoutItem>();
        var rows = new List<LayoutRow>();
        int sequence = 1;

        foreach (var piece in pieces)
        {
            var width = piece.Width;
            var length = piece.Length;
            var rotated = false;

            if (width > usableWidth && length <= usableWidth)
            {
                (width, length) = (length, width);
                rotated = true;
            }

            if (width > usableWidth)
            {
                continue;
            }

            LayoutRow? targetRow = null;
            foreach (var row in rows)
            {
                if (row.RemainingWidth >= width + (row.Items.Any() ? innerSpacing : 0))
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

                targetRow = new LayoutRow
                {
                    YPosition = newRowY,
                    Height = 0,
                    RemainingWidth = usableWidth,
                    CurrentX = outerSpacing
                };
                rows.Add(targetRow);
            }

            var xPosition = targetRow.CurrentX;
            if (targetRow.Items.Any())
            {
                xPosition += innerSpacing;
            }

            var layoutItem = new LayoutItem
            {
                LayoutId = layoutId,
                BarcodeNo = piece.BarcodeNo,
                XPosition = xPosition,
                YPosition = targetRow.YPosition,
                Width = width,
                Length = length,
                IsRotated = rotated,
                AreaSqm = piece.Sqm,
                OrderNo = piece.OrderNo,
                OrderType = piece.OrderType,
                Sequence = sequence++
            };

            layoutItems.Add(layoutItem);
            targetRow.Items.Add(layoutItem);

            targetRow.CurrentX = xPosition + width;
            targetRow.RemainingWidth = usableWidth - (targetRow.CurrentX - outerSpacing);
            if (length > targetRow.Height)
            {
                targetRow.Height = length;
            }
        }

        return layoutItems;
    }

    private class LayoutRow
    {
        public decimal YPosition { get; set; }
        public decimal Height { get; set; }
        public decimal RemainingWidth { get; set; }
        public decimal CurrentX { get; set; }
        public List<LayoutItem> Items { get; set; } = new();
    }

    private class LayoutRowStateless
    {
        public decimal YPosition { get; set; }
        public decimal Height { get; set; }
        public decimal RemainingWidth { get; set; }
        public decimal CurrentX { get; set; }
        public List<PlacedItem> Items { get; set; } = new();
    }

    private class PlacedItem
    {
        public string BarcodeNo { get; set; } = string.Empty;
        public string OrderNo { get; set; } = string.Empty;
        public string OrderType { get; set; } = string.Empty;
        public decimal XPosition { get; set; }
        public decimal YPosition { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
        public bool IsRotated { get; set; }
    }
}
