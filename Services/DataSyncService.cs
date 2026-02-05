using Microsoft.EntityFrameworkCore;
using CarpetOpsSystem.Data;
using CarpetOpsSystem.Models;

namespace CarpetOpsSystem.Services;

public class DataSyncService
{
    private readonly SqlServerDataReader _sqlServerReader;
    private readonly PostgresContext _postgresContext;

    public DataSyncService(SqlServerDataReader sqlServerReader, PostgresContext postgresContext)
    {
        _sqlServerReader = sqlServerReader;
        _postgresContext = postgresContext;
    }

    public async Task<SyncResult> SyncAllAsync(DateTime? fromDate = null)
    {
        var result = new SyncResult();

        var sourceData = await _sqlServerReader.FetchFabricDataAsync(fromDate);
        sourceData = sourceData
            .Where(x => !string.IsNullOrWhiteSpace(x.BarcodeNo))
            .GroupBy(x => x.BarcodeNo)
            .Select(g => g.First())
            .ToList();

        result.TotalSourceRecords = sourceData.Count;

        if (sourceData.Count == 0)
        {
            result.Success = true;
            result.SyncedAt = DateTime.UtcNow;
            return result;
        }

        _postgresContext.ChangeTracker.AutoDetectChangesEnabled = false;

        const int batchSize = 1000;
        var totalInserted = 0;
        var totalUpdated = 0;

        try
        {
            for (var offset = 0; offset < sourceData.Count; offset += batchSize)
            {
                var batch = sourceData.Skip(offset).Take(batchSize).ToList();
                var barcodes = batch.Select(x => x.BarcodeNo).Distinct().ToList();

                var existingMap = await _postgresContext.FabricPieces
                    .Where(f => barcodes.Contains(f.BarcodeNo))
                    .ToDictionaryAsync(f => f.BarcodeNo);

                foreach (var source in batch)
                {
                    if (existingMap.TryGetValue(source.BarcodeNo, out var existing))
                    {
                        existing.OrderNo = source.ORNO;
                        existing.ListNo = source.ListNo;
                        existing.ItemNo = source.ITEM_NO;
                        existing.CnvId = source.CnvID;
                        existing.CnvDesc = source.CnvDesc;
                        existing.AsPlan = source.ASPLAN;
                        existing.Width = source.Width;
                        existing.Length = source.Length;
                        existing.Sqm = source.Sqm;
                        existing.Qty = source.Qty;
                        existing.OrderType = source.OrderType;
                        existing.SyncedAt = DateTime.UtcNow;

                        totalUpdated++;
                    }
                    else
                    {
                        var fabricPiece = source.ToFabricPiece();
                        fabricPiece.SyncedAt = DateTime.UtcNow;
                        _postgresContext.FabricPieces.Add(fabricPiece);
                        totalInserted++;
                    }
                }

                await _postgresContext.SaveChangesAsync();
                _postgresContext.ChangeTracker.Clear();
            }
        }
        catch (DbUpdateException ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            return result;
        }
        finally
        {
            _postgresContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        result.InsertedCount = totalInserted;
        result.UpdatedCount = totalUpdated;
        result.Success = true;
        result.SyncedAt = DateTime.UtcNow;

        return result;
    }

    public async Task<SyncResult> SyncByOrderNumbersAsync(IEnumerable<string> orderNumbers)
    {
        var result = new SyncResult();

        var sourceData = await _sqlServerReader.FetchByOrderNumbersAsync(orderNumbers);
        sourceData = sourceData
            .Where(x => !string.IsNullOrWhiteSpace(x.BarcodeNo))
            .GroupBy(x => x.BarcodeNo)
            .Select(g => g.First())
            .ToList();

        result.TotalSourceRecords = sourceData.Count;

        if (sourceData.Count == 0)
        {
            result.Success = true;
            result.SyncedAt = DateTime.UtcNow;
            return result;
        }

        _postgresContext.ChangeTracker.AutoDetectChangesEnabled = false;

        const int batchSize = 1000;
        var totalInserted = 0;
        var totalUpdated = 0;

        try
        {
            for (var offset = 0; offset < sourceData.Count; offset += batchSize)
            {
                var batch = sourceData.Skip(offset).Take(batchSize).ToList();
                var barcodes = batch.Select(x => x.BarcodeNo).Distinct().ToList();

                var existingMap = await _postgresContext.FabricPieces
                    .Where(f => barcodes.Contains(f.BarcodeNo))
                    .ToDictionaryAsync(f => f.BarcodeNo);

                foreach (var source in batch)
                {
                    if (existingMap.TryGetValue(source.BarcodeNo, out var existing))
                    {
                        existing.OrderNo = source.ORNO;
                        existing.ListNo = source.ListNo;
                        existing.ItemNo = source.ITEM_NO;
                        existing.CnvId = source.CnvID;
                        existing.CnvDesc = source.CnvDesc;
                        existing.AsPlan = source.ASPLAN;
                        existing.Width = source.Width;
                        existing.Length = source.Length;
                        existing.Sqm = source.Sqm;
                        existing.Qty = source.Qty;
                        existing.OrderType = source.OrderType;
                        existing.SyncedAt = DateTime.UtcNow;

                        totalUpdated++;
                    }
                    else
                    {
                        var fabricPiece = source.ToFabricPiece();
                        fabricPiece.SyncedAt = DateTime.UtcNow;
                        _postgresContext.FabricPieces.Add(fabricPiece);
                        totalInserted++;
                    }
                }

                await _postgresContext.SaveChangesAsync();
                _postgresContext.ChangeTracker.Clear();
            }
        }
        catch (DbUpdateException ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
            return result;
        }
        finally
        {
            _postgresContext.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        result.InsertedCount = totalInserted;
        result.UpdatedCount = totalUpdated;
        result.Success = true;
        result.SyncedAt = DateTime.UtcNow;

        return result;
    }
}

public class SyncResult
{
    public bool Success { get; set; }
    public int TotalSourceRecords { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public DateTime SyncedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
