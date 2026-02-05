using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Config;
using CarpetOpsSystem.Data;
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
}
