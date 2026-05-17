using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestHistoryData.TestUtilities;

namespace TestHistoryData.Phase7;

public class CandlesMapperTests
{
    private static MapContext CreateTestContext(
        string sourceCode = "MOEX_ALGOPACK",
        string timezone = "Europe/Moscow")
        => new(
            SourceCode: sourceCode,
            Endpoint: "/datashop/algopack/eq/candles/SBER.json",
            SourceTimezone: timezone,
            LoadJobId: Guid.CreateVersion7(),
            RawObjectId: Guid.CreateVersion7(),
            FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    private static readonly TimeZoneInfo MoscowTz =
        SourceTimezones.Resolve("Europe/Moscow");

    [Fact]
    public void Map_MskToUtc_ConvertsCorrectly()
    {
        var dto = new CandlesDTO { Begin = new DateTime(2026, 5, 7, 10, 0, 0) };
        var ctx = CreateTestContext();

        Candle1m candle = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        DateTime expectedUtc = new DateTime(2026, 5, 7, 7, 0, 0, DateTimeKind.Utc);
        Assert.Equal(expectedUtc, candle.BeginUtc);
        Assert.Equal(DateTimeKind.Utc, candle.BeginUtc.Kind);
    }

    [Fact]
    public void Map_BeginLocal_PreservesOriginal()
    {
        var dto = new CandlesDTO { Begin = new DateTime(2026, 5, 7, 10, 0, 0) };
        var ctx = CreateTestContext();

        Candle1m candle = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        Assert.Equal(dto.Begin!.Value, candle.BeginLocal);
    }

    [Fact]
    public void Map_SameDto_SameHash()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = 300.5, High = 301.0, Low = 299.0, Close = 300.8,
            Volume = 1000, Value = 300500
        };
        var ctx = CreateTestContext();

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        Assert.Equal(candle1.RowHashV1, candle2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = 300.5, High = 301.0, Low = 299.0, Close = 300.8
        };
        var ctx1 = CreateTestContext("MOEX_ALGOPACK");
        var ctx2 = CreateTestContext("MOEX_ISS");

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx1, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "SBER", ctx2, MoscowTz);

        Assert.NotEqual(candle1.RowHashV1, candle2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSecId_DifferentHash()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = 300.5, High = 301.0, Low = 299.0, Close = 300.8
        };
        var ctx = CreateTestContext();

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "GAZP", ctx, MoscowTz);

        Assert.NotEqual(candle1.RowHashV1, candle2.RowHashV1);
    }

    [Fact]
    public void Map_NullPrice_HashStable()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = null, High = 301.0, Low = 299.0, Close = 300.8
        };
        var ctx = CreateTestContext();

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        Assert.Equal(candle1.RowHashV1, candle2.RowHashV1);
    }

    // ── Phase 7.5 §9.6 additions ──────────────────────────────────────────

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = 300.5, High = 301.0, Low = 299.0, Close = 300.8,
            Volume = 1000, Value = 300500,
        };
        var ctx1 = CreateTestContext();
        var ctx2 = CreateTestContext();

        Assert.NotEqual(ctx1.LoadJobId, ctx2.LoadJobId);
        Assert.NotEqual(ctx1.RawObjectId, ctx2.RawObjectId);

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx1, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "SBER", ctx2, MoscowTz);

        Assert.Equal(candle1.RowHashV1, candle2.RowHashV1);
    }

    [Fact]
    public void Map_BeginLocal_KindIsUnspecified()
    {
        DateTime utcBegin = DateTime.SpecifyKind(
            new DateTime(2026, 5, 7, 10, 0, 0), DateTimeKind.Utc);
        var dto = new CandlesDTO { Begin = utcBegin };
        var ctx = CreateTestContext();

        Candle1m candle = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        Assert.Equal(DateTimeKind.Unspecified, candle.BeginLocal.Kind);
    }

    [Fact]
    public void Map_NullBegin_ThrowsMappingValidationException()
    {
        var dto = new CandlesDTO { Begin = null };
        var ctx = CreateTestContext();

        var ex = Assert.Throws<MappingValidationException>(
            () => CandlesMapper.Map(dto, "SBER", ctx, MoscowTz, rowIndex: 5));

        Assert.Equal("mapping_validation", ex.ErrorCategory);
        Assert.Equal("candles", ex.Category);
        Assert.Equal("SBER", ex.SecId);
        Assert.Equal(5, ex.RowIndex);
    }

    [Fact]
    public void MapBatch_WithLogger_DoesNotThrow()
    {
        var dtos = new List<CandlesDTO>
        {
            new() { Begin = new DateTime(2026, 5, 7, 10, 0, 0), Open = 100, High = 101, Low = 99, Close = 100 },
            new() { Begin = new DateTime(2026, 5, 7, 10, 1, 0), Open = 100, High = 101, Low = 99, Close = 100 },
        };
        var ctx = CreateTestContext();

        List<Candle1m> result = CandlesMapper.MapBatch(
            dtos, "SBER", ctx, NullLogger.Instance);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void MapBatch_Cancelled_LogsAndRethrows()
    {
        var dtos = new List<CandlesDTO>
        {
            new() { Begin = new DateTime(2026, 5, 7, 10, 0, 0), Open = 100 },
        };
        var ctx = CreateTestContext();
        var logger = new ListLogger<CandlesMapperTests>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.Throws<OperationCanceledException>(
            () => CandlesMapper.MapBatch(dtos, "SBER", ctx, logger, cts.Token));

        Assert.Contains(logger.Entries, e => e.EventId.Id == 213);
    }

    [Fact]
    public void MapBatch_TimezoneResolveFails_LogsMapBatchFailed()
    {
        var dtos = new List<CandlesDTO>
        {
            new() { Begin = new DateTime(2026, 5, 7, 10, 0, 0), Open = 100 },
        };
        var ctx = CreateTestContext(timezone: "Invalid/Tz");
        var logger = new ListLogger<CandlesMapperTests>();

        Assert.Throws<MappingTimezoneException>(
            () => CandlesMapper.MapBatch(dtos, "SBER", ctx, logger));

        Assert.Contains(logger.Entries, e => e.EventId.Id == 214);
        Assert.DoesNotContain(logger.Entries, e => e.EventId.Id == 212);
    }
}
