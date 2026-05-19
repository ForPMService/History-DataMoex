using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarFuturesSessionMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarFuturesSessionDTO> { MakeValidCalendarFuturesSessionDto() };
        var ctx = MakeContext();

        List<CalendarFuturesSession> result = CalendarFuturesSessionMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarFuturesSession row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Null(row.SecId);
        Assert.Equal(new DateOnly(2026, 5, 17), row.TradeSessionDate);
        if (row.TimeFromUtc.HasValue)
            Assert.Equal(DateTimeKind.Utc, row.TimeFromUtc.Value.Kind);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarFuturesSessionMapper.MapBatch(
            new List<CalendarFuturesSessionDTO> { MakeValidCalendarFuturesSessionDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarFuturesSessionMapper.MapBatch(
            new List<CalendarFuturesSessionDTO> { MakeValidCalendarFuturesSessionDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarFuturesSessionDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarFuturesSessionDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarFuturesSessionDto();
        var ctx = MakeContext();

        var r = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_SecIdEmpty_NormalizesToNull()
    {
        var dto = MakeValidCalendarFuturesSessionDto(secId: "");
        var ctx = MakeContext();

        var r = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Null(r[0].SecId);
    }

    [Fact]
    public void Map_SecIdDash_NormalizesToNull()
    {
        var dto = MakeValidCalendarFuturesSessionDto(secId: "-");
        var ctx = MakeContext();

        var r = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Null(r[0].SecId);
    }

    [Fact]
    public void Map_SecIdValid_PassedThrough()
    {
        var dto = MakeValidCalendarFuturesSessionDto(secId: "SiM6");
        var ctx = MakeContext();

        var r = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal("SiM6", r[0].SecId);
    }

    [Fact]
    public void Map_TimeFromTimeTillDateTime_ConversionMskToUtc()
    {
        // 2026-05-17 10:00 MSK = 07:00 UTC; 18:50 MSK = 15:50 UTC.
        var dto = MakeValidCalendarFuturesSessionDto() with
        {
            TimeFrom = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Unspecified),
            TimeTill = new DateTime(2026, 5, 17, 18, 50, 0, DateTimeKind.Unspecified),
        };
        var ctx = MakeContext();

        var r = CalendarFuturesSessionMapper.MapBatch(new List<CalendarFuturesSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.NotNull(r[0].TimeFromUtc);
        Assert.NotNull(r[0].TimeTillUtc);
        Assert.Equal(new DateTime(2026, 5, 17, 7, 0, 0, DateTimeKind.Utc), r[0].TimeFromUtc!.Value);
        Assert.Equal(new DateTime(2026, 5, 17, 15, 50, 0, DateTimeKind.Utc), r[0].TimeTillUtc!.Value);
        Assert.Equal(DateTimeKind.Utc, r[0].TimeFromUtc!.Value.Kind);
    }
}
