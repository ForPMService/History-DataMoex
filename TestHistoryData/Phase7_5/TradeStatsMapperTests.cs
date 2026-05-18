using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using TestHistoryData.TestUtilities;
using static TestHistoryData.Phase7_5.MapperTestsHelpers;

namespace TestHistoryData.Phase7_5;

public class TradeStatsMapperTests
{
    // ── Smoke (phase 7.5-B) ──────────────────────────────────────────────

    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<SuperCandlesTradeStats5mDTO>
        {
            MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER"),
        };

        List<TradeStats5m> result = TradeStatsMapper.MapBatch(
            dtos, "SBER", MakeContext("MOEX_ALGOPACK", "Europe/Moscow"), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SBER", result[0].SecId);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
        Assert.Equal(300, result[0].IntervalSeconds);
    }

    // ── B1-B5: базовые тесты ─────────────────────────────────────────────

    [Fact]
    public void Map_TimezoneConversion()
    {
        var dto = MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        TradeStats5m model = TradeStatsMapper.Map(dto, "SBER", ctx, tz);

        Assert.Equal(new DateTime(2026, 5, 7, 7, 5, 0, DateTimeKind.Utc), model.BeginUtc);
        Assert.Equal(new DateTime(2026, 5, 7, 10, 5, 0), model.BeginLocal);
        Assert.Equal(DateTimeKind.Utc, model.BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, model.BeginLocal.Kind);
    }

    [Fact]
    public void Map_HashStability()
    {
        var dto = MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        TradeStats5m m1 = TradeStatsMapper.Map(dto, "SBER", ctx, tz);
        TradeStats5m m2 = TradeStatsMapper.Map(dto, "SBER", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctxAlg = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctxIss = ctxAlg with { SourceCode = "MOEX_ISS" };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctxAlg.SourceTimezone);

        TradeStats5m m1 = TradeStatsMapper.Map(dto, "SBER", ctxAlg, tz);
        TradeStats5m m2 = TradeStatsMapper.Map(dto, "SBER", ctxIss, tz);

        Assert.NotEqual(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_NullFields_HashStable()
    {
        var dto = MakeMinimalTradeStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        TradeStats5m m1 = TradeStatsMapper.Map(dto, "SBER", ctx, tz);
        TradeStats5m m2 = TradeStatsMapper.Map(dto, "SBER", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(0UL, m1.RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctx1 = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctx2 = ctx1 with
        {
            RawObjectId = Guid.CreateVersion7(),
            LoadJobId = Guid.CreateVersion7(),
        };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx1.SourceTimezone);

        TradeStats5m m1 = TradeStatsMapper.Map(dto, "SBER", ctx1, tz);
        TradeStats5m m2 = TradeStatsMapper.Map(dto, "SBER", ctx2, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(m1.RawObjectId, m2.RawObjectId);
        Assert.NotEqual(m1.LoadJobId, m2.LoadJobId);
    }

    // ── SecId mismatch ───────────────────────────────────────────────────

    [Fact]
    public void Map_DtoSecIdMismatch_ThrowsMappingValidationException()
    {
        var dto = MakeValidTradeStatsDto("2026-05-07", "10:05:00", "GAZP");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        var ex = Assert.Throws<MappingValidationException>(() =>
            TradeStatsMapper.Map(dto, "SBER", ctx, tz, rowIndex: 3));

        Assert.Equal("mapping_validation", ex.ErrorCategory);
        Assert.Equal(MappingCategories.TradeStats, ex.Category);
        Assert.Equal("SBER", ex.SecId);
        Assert.Equal(3, ex.RowIndex);
    }

    // ── MapBatch контракты (этот файл представляет паттерн для всех 10) ──

    [Fact]
    public void MapBatch_WithLogger_DoesNotThrow()
    {
        var dtos = new List<SuperCandlesTradeStats5mDTO>
        {
            MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER"),
        };
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");

        List<TradeStats5m> result = TradeStatsMapper.MapBatch(
            dtos, "SBER", ctx, NullLogger.Instance);

        Assert.Single(result);
    }

    [Fact]
    public void MapBatch_Cancelled_LogsAndRethrows()
    {
        var dtos = Enumerable.Range(0, 5000)
            .Select(_ => MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER"))
            .ToList();
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var logger = new ListLogger<TradeStatsMapperTests>();

        Assert.Throws<OperationCanceledException>(() =>
            TradeStatsMapper.MapBatch(dtos, "SBER", ctx, logger, cts.Token));

        Assert.Contains(logger.Entries, e => e.EventId.Id == 213);
    }

    [Fact]
    public void MapBatch_TimezoneResolveFails_LogsMapBatchFailed_NotMapRowFailed()
    {
        var dtos = new List<SuperCandlesTradeStats5mDTO>
        {
            MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER"),
        };
        var ctx = MakeContext("MOEX_ALGOPACK", "Invalid/NotAZone");

        var logger = new ListLogger<TradeStatsMapperTests>();

        Assert.Throws<MappingTimezoneException>(() =>
            TradeStatsMapper.MapBatch(dtos, "SBER", ctx, logger));

        Assert.Contains(logger.Entries, e => e.EventId.Id == 214);
        Assert.DoesNotContain(logger.Entries, e => e.EventId.Id == 212);
    }

    [Fact]
    public void MapBatch_RowMapFails_LogsMapRowFailed_NotMapBatchFailed()
    {
        var dtos = new List<SuperCandlesTradeStats5mDTO>
        {
            MakeValidTradeStatsDto("2026-05-07", "10:05:00", "GAZP"),
        };
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");

        var logger = new ListLogger<TradeStatsMapperTests>();

        Assert.Throws<MappingValidationException>(() =>
            TradeStatsMapper.MapBatch(dtos, "SBER", ctx, logger));

        Assert.Contains(logger.Entries, e => e.EventId.Id == 212);
        Assert.DoesNotContain(logger.Entries, e => e.EventId.Id == 214);
    }

    [Fact]
    public void MapBatch_HelperRowException_LogsCorrectRowIndex()
    {
        var dtos = new List<SuperCandlesTradeStats5mDTO>
        {
            MakeValidTradeStatsDto("2026-05-07", "10:05:00", "SBER"),
            MakeValidTradeStatsDto("2026-05-07", "10:10:00", "SBER"),
            MakeValidTradeStatsDto(null,         "10:15:00", "SBER"),
        };
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");

        var logger = new ListLogger<TradeStatsMapperTests>();

        Assert.Throws<MappingDateTimeException>(() =>
            TradeStatsMapper.MapBatch(dtos, "SBER", ctx, logger));

        var rowFailedEntry = logger.Entries.Single(e => e.EventId.Id == 212);
        Assert.Contains("rowIndex=2", rowFailedEntry.Message);
        Assert.Contains("mapping_datetime", rowFailedEntry.Message);
    }
}
