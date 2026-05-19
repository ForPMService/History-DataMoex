using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarOptionsSeriesMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarOptionsSeriesDTO> { MakeValidCalendarOptionsSeriesDto() };
        var ctx = MakeContext();

        List<CalendarOptionsSeries> result = CalendarOptionsSeriesMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarOptionsSeries row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal("Si", row.AssetCode);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarOptionsSeriesMapper.MapBatch(
            new List<CalendarOptionsSeriesDTO> { MakeValidCalendarOptionsSeriesDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarOptionsSeriesMapper.MapBatch(
            new List<CalendarOptionsSeriesDTO> { MakeValidCalendarOptionsSeriesDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarOptionsSeriesDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarOptionsSeriesMapper.MapBatch(new List<CalendarOptionsSeriesDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarOptionsSeriesMapper.MapBatch(new List<CalendarOptionsSeriesDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarOptionsSeriesDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarOptionsSeriesMapper.MapBatch(new List<CalendarOptionsSeriesDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarOptionsSeriesMapper.MapBatch(new List<CalendarOptionsSeriesDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarOptionsSeriesDto();
        var ctx = MakeContext();

        var r = CalendarOptionsSeriesMapper.MapBatch(new List<CalendarOptionsSeriesDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_NoSecIdField_ProcessesGroupCWithoutSecIdValidation()
    {
        // Group C — модель не имеет SecId. Любой dto без AssetCode проходит, lineage есть.
        var dto = MakeValidCalendarOptionsSeriesDto() with { AssetCode = null };
        var ctx = MakeContext();

        var r = CalendarOptionsSeriesMapper.MapBatch(new List<CalendarOptionsSeriesDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Single(r);
        Assert.Null(r[0].AssetCode);
    }

    [Fact]
    public void Map_ExpirationDateExpirationTime_ParsesIsoStrings()
    {
        var dto = MakeValidCalendarOptionsSeriesDto() with
        {
            ExpirationDate = "2026-06-18",
            ExpirationTime = "18:45:00",
        };
        var ctx = MakeContext();

        var r = CalendarOptionsSeriesMapper.MapBatch(new List<CalendarOptionsSeriesDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(new DateOnly(2026, 6, 18), r[0].ExpirationDate);
        Assert.Equal(new TimeOnly(18, 45, 0), r[0].ExpirationTime);
    }

    [Fact]
    public void Map_WeekendSession_PassedThrough()
    {
        var dto = MakeValidCalendarOptionsSeriesDto() with { WeekendSession = 1 };
        var ctx = MakeContext();

        var r = CalendarOptionsSeriesMapper.MapBatch(new List<CalendarOptionsSeriesDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(1, r[0].WeekendSession);
    }
}
