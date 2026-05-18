using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase7_5.MapperTestsHelpers;

namespace TestHistoryData.Phase7_5;

public class FuturesOrderBookStatsMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<SuperCandlesFuturesOrderBookStats5mDTO>
        {
            MakeValidFuturesOrderBookStatsDto("2026-05-07", "10:05:00", "SiM5"),
        };

        List<FuturesOrderBookStats5m> result = FuturesOrderBookStatsMapper.MapBatch(
            dtos, "SiM5", MakeContext("MOEX_ALGOPACK", "Europe/Moscow"), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SiM5", result[0].SecId);
        Assert.Equal("Si", result[0].AssetCode);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
        Assert.Equal(300, result[0].IntervalSeconds);
    }

    [Fact]
    public void Map_TimezoneConversion()
    {
        var dto = MakeValidFuturesOrderBookStatsDto("2026-05-07", "10:05:00", "SiM5");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        FuturesOrderBookStats5m model = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctx, tz);

        Assert.Equal(new DateTime(2026, 5, 7, 7, 5, 0, DateTimeKind.Utc), model.BeginUtc);
        Assert.Equal(new DateTime(2026, 5, 7, 10, 5, 0), model.BeginLocal);
        Assert.Equal(DateTimeKind.Utc, model.BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, model.BeginLocal.Kind);
    }

    [Fact]
    public void Map_HashStability()
    {
        var dto = MakeValidFuturesOrderBookStatsDto("2026-05-07", "10:05:00", "SiM5");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        FuturesOrderBookStats5m m1 = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctx, tz);
        FuturesOrderBookStats5m m2 = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidFuturesOrderBookStatsDto("2026-05-07", "10:05:00", "SiM5");
        var ctxAlg = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctxIss = ctxAlg with { SourceCode = "MOEX_ISS" };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctxAlg.SourceTimezone);

        FuturesOrderBookStats5m m1 = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctxAlg, tz);
        FuturesOrderBookStats5m m2 = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctxIss, tz);

        Assert.NotEqual(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_NullFields_HashStable()
    {
        var dto = MakeMinimalFuturesOrderBookStatsDto("2026-05-07", "10:05:00", "SiM5");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        FuturesOrderBookStats5m m1 = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctx, tz);
        FuturesOrderBookStats5m m2 = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(0UL, m1.RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidFuturesOrderBookStatsDto("2026-05-07", "10:05:00", "SiM5");
        var ctx1 = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctx2 = ctx1 with
        {
            RawObjectId = Guid.CreateVersion7(),
            LoadJobId = Guid.CreateVersion7(),
        };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx1.SourceTimezone);

        FuturesOrderBookStats5m m1 = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctx1, tz);
        FuturesOrderBookStats5m m2 = FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctx2, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(m1.RawObjectId, m2.RawObjectId);
        Assert.NotEqual(m1.LoadJobId, m2.LoadJobId);
    }

    [Fact]
    public void Map_DtoSecIdMismatch_ThrowsMappingValidationException()
    {
        var dto = MakeValidFuturesOrderBookStatsDto("2026-05-07", "10:05:00", "BRM5");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        var ex = Assert.Throws<MappingValidationException>(() =>
            FuturesOrderBookStatsMapper.Map(dto, "SiM5", ctx, tz, rowIndex: 11));

        Assert.Equal("mapping_validation", ex.ErrorCategory);
        Assert.Equal(MappingCategories.FuturesOrderBookStats, ex.Category);
        Assert.Equal("SiM5", ex.SecId);
        Assert.Equal(11, ex.RowIndex);
    }
}
