using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase7_5.MapperTestsHelpers;

namespace TestHistoryData.Phase7_5;

public class OrderBookStatsMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<SuperCandlesOrderBookStats5mDTO>
        {
            MakeValidOrderBookStatsDto("2026-05-07", "10:05:00", "SBER"),
        };

        List<OrderBookStats5m> result = OrderBookStatsMapper.MapBatch(
            dtos, "SBER", MakeContext("MOEX_ALGOPACK", "Europe/Moscow"), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SBER", result[0].SecId);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
        Assert.Equal(300, result[0].IntervalSeconds);
    }

    [Fact]
    public void Map_TimezoneConversion()
    {
        var dto = MakeValidOrderBookStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        OrderBookStats5m model = OrderBookStatsMapper.Map(dto, "SBER", ctx, tz);

        Assert.Equal(new DateTime(2026, 5, 7, 7, 5, 0, DateTimeKind.Utc), model.BeginUtc);
        Assert.Equal(new DateTime(2026, 5, 7, 10, 5, 0), model.BeginLocal);
        Assert.Equal(DateTimeKind.Utc, model.BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, model.BeginLocal.Kind);
    }

    [Fact]
    public void Map_HashStability()
    {
        var dto = MakeValidOrderBookStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        OrderBookStats5m m1 = OrderBookStatsMapper.Map(dto, "SBER", ctx, tz);
        OrderBookStats5m m2 = OrderBookStatsMapper.Map(dto, "SBER", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidOrderBookStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctxAlg = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctxIss = ctxAlg with { SourceCode = "MOEX_ISS" };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctxAlg.SourceTimezone);

        OrderBookStats5m m1 = OrderBookStatsMapper.Map(dto, "SBER", ctxAlg, tz);
        OrderBookStats5m m2 = OrderBookStatsMapper.Map(dto, "SBER", ctxIss, tz);

        Assert.NotEqual(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_NullFields_HashStable()
    {
        var dto = MakeMinimalOrderBookStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        OrderBookStats5m m1 = OrderBookStatsMapper.Map(dto, "SBER", ctx, tz);
        OrderBookStats5m m2 = OrderBookStatsMapper.Map(dto, "SBER", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(0UL, m1.RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidOrderBookStatsDto("2026-05-07", "10:05:00", "SBER");
        var ctx1 = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctx2 = ctx1 with
        {
            RawObjectId = Guid.CreateVersion7(),
            LoadJobId = Guid.CreateVersion7(),
        };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx1.SourceTimezone);

        OrderBookStats5m m1 = OrderBookStatsMapper.Map(dto, "SBER", ctx1, tz);
        OrderBookStats5m m2 = OrderBookStatsMapper.Map(dto, "SBER", ctx2, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(m1.RawObjectId, m2.RawObjectId);
        Assert.NotEqual(m1.LoadJobId, m2.LoadJobId);
    }

    [Fact]
    public void Map_DtoSecIdMismatch_ThrowsMappingValidationException()
    {
        var dto = MakeValidOrderBookStatsDto("2026-05-07", "10:05:00", "GAZP");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        var ex = Assert.Throws<MappingValidationException>(() =>
            OrderBookStatsMapper.Map(dto, "SBER", ctx, tz, rowIndex: 4));

        Assert.Equal("mapping_validation", ex.ErrorCategory);
        Assert.Equal(MappingCategories.OrderBookStats, ex.Category);
        Assert.Equal("SBER", ex.SecId);
        Assert.Equal(4, ex.RowIndex);
    }
}
