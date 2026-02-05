using System.Data;
using Microsoft.Data.SqlClient;
using CarpetOpsSystem.Models;
using CarpetOpsSystem.Config;

namespace CarpetOpsSystem.Data;

public class SqlServerDataReader
{
    private readonly string _connectionString;

    public SqlServerDataReader(AppSettings settings)
    {
        _connectionString = settings.SqlServerConnectionString;
    }

    public async Task<List<SourceFabricData>> FetchFabricDataAsync(DateTime? fromDate = null)
    {
        var startDate = fromDate ?? new DateTime(2026, 1, 1);
        var results = new List<SourceFabricData>();

        const string query = @"
            SELECT
                C.HT_BARCODE AS BarcodeNo,
                A.ORNO,
                B.PD_ITEM AS ListNo,
                C.ITEM_NO,
                B.CnvID,
                B.CnvDesc,
                B.AsPlan AS ASPLAN,
                B.PD_WIDTH AS Width,
                B.PD_LEN AS Length,
                C.SQM AS Sqm,
                C.Qty AS Qty,
                CASE WHEN A.ORTP = '0' THEN 'Order' ELSE 'Sample' END AS OrderType
            FROM CARPET.DBO.ORHMAIN A
            LEFT JOIN CARPET.DBO.ORDMAIN B ON A.AUTO = B.AUTO
            LEFT JOIN CARPET.DBO.HT_SCANBARCODE C ON C.OF_NO = A.ORNO
            WHERE A.ORDT >= @StartDate
                AND C.HT_BARCODE IS NOT NULL
                AND ISNULL(B.CnvID,'') <> ''";

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@StartDate", startDate);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new SourceFabricData
            {
                BarcodeNo = GetString(reader, "BarcodeNo") ?? string.Empty,
                ORNO = GetString(reader, "ORNO") ?? string.Empty,
                ListNo = GetInt32(reader, "ListNo") ?? 0,
                ITEM_NO = GetInt32(reader, "ITEM_NO") ?? 0,
                CnvID = GetString(reader, "CnvID") ?? string.Empty,
                CnvDesc = GetString(reader, "CnvDesc"),
                ASPLAN = GetString(reader, "ASPLAN"),
                Width = GetDecimal(reader, "Width") ?? 0m,
                Length = GetDecimal(reader, "Length") ?? 0m,
                Sqm = GetDecimal(reader, "Sqm") ?? 0m,
                Qty = GetInt32(reader, "Qty") ?? 0,
                OrderType = GetString(reader, "OrderType") ?? string.Empty
            });
        }

        return results;
    }

    public async Task<List<SourceFabricData>> FetchByOrderNumbersAsync(IEnumerable<string> orderNumbers)
    {
        var orders = (orderNumbers ?? Array.Empty<string>())
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.Trim())
            .Distinct()
            .ToList();

        if (orders.Count == 0)
            return new List<SourceFabricData>();

        var parameters = orders.Select((_, i) => $"@o{i}").ToArray();
        var inClause = string.Join(",", parameters);

        var query = $@"
            SELECT
                C.HT_BARCODE AS BarcodeNo,
                A.ORNO,
                B.PD_ITEM AS ListNo,
                C.ITEM_NO,
                B.CnvID,
                B.CnvDesc,
                B.AsPlan AS ASPLAN,
                B.PD_WIDTH AS Width,
                B.PD_LEN AS Length,
                C.SQM AS Sqm,
                C.Qty AS Qty,
                CASE WHEN A.ORTP = '0' THEN 'Order' ELSE 'Sample' END AS OrderType
            FROM CARPET.DBO.ORHMAIN A
            LEFT JOIN CARPET.DBO.ORDMAIN B ON A.AUTO = B.AUTO
            LEFT JOIN CARPET.DBO.HT_SCANBARCODE C ON C.OF_NO = A.ORNO
            WHERE A.ORNO IN ({inClause})
                AND C.HT_BARCODE IS NOT NULL
                AND ISNULL(B.CnvID,'') <> ''";

        var results = new List<SourceFabricData>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(query, connection);
        for (var i = 0; i < orders.Count; i++)
            command.Parameters.AddWithValue(parameters[i], orders[i]);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new SourceFabricData
            {
                BarcodeNo = GetString(reader, "BarcodeNo") ?? string.Empty,
                ORNO = GetString(reader, "ORNO") ?? string.Empty,
                ListNo = GetInt32(reader, "ListNo") ?? 0,
                ITEM_NO = GetInt32(reader, "ITEM_NO") ?? 0,
                CnvID = GetString(reader, "CnvID") ?? string.Empty,
                CnvDesc = GetString(reader, "CnvDesc"),
                ASPLAN = GetString(reader, "ASPLAN"),
                Width = GetDecimal(reader, "Width") ?? 0m,
                Length = GetDecimal(reader, "Length") ?? 0m,
                Sqm = GetDecimal(reader, "Sqm") ?? 0m,
                Qty = GetInt32(reader, "Qty") ?? 0,
                OrderType = GetString(reader, "OrderType") ?? string.Empty
            });
        }

        return results;
    }

    private static int? GetInt32(SqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal)) return null;

        var value = reader.GetValue(ordinal);

        if (value is int i) return i;
        if (value is long l) return checked((int)l);
        if (value is short s) return s;
        if (value is byte b) return b;
        if (value is decimal d) return (int)d;
        if (value is double db) return (int)db;
        if (value is float f) return (int)f;

        var str = value.ToString();
        if (string.IsNullOrWhiteSpace(str)) return null;

        return int.TryParse(str, out var parsed) ? parsed : null;
    }

    private static decimal? GetDecimal(SqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal)) return null;

        var value = reader.GetValue(ordinal);

        if (value is decimal d) return d;
        if (value is double db) return (decimal)db;
        if (value is float f) return (decimal)f;
        if (value is int i) return i;
        if (value is long l) return l;
        if (value is short s) return s;
        if (value is byte b) return b;

        var str = value.ToString();
        if (string.IsNullOrWhiteSpace(str)) return null;

        return decimal.TryParse(str, out var parsed) ? parsed : null;
    }

    private static string? GetString(SqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        if (reader.IsDBNull(ordinal)) return null;

        var value = reader.GetValue(ordinal);

        if (value is string s) return s;
        return value.ToString();
    }
}
