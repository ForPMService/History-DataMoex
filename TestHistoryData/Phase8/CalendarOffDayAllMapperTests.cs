using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarOffDayAllMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarOffDaysAllDTO> { MakeValidCalendarOffDayAllDto() };
        var ctx = MakeContext();

        List<CalendarOffDayAll> result = CalendarOffDayAllMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarOffDayAll row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal(new DateOnly(2026, 5, 9), row.TradeDate);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarOffDayAllMapper.MapBatch(
            new List<CalendarOffDaysAllDTO> { MakeValidCalendarOffDayAllDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarOffDayAllMapper.MapBatch(
            new List<CalendarOffDaysAllDTO> { MakeValidCalendarOffDayAllDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarOffDayAllDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarOffDayAllDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarOffDayAllDto();
        var ctx = MakeContext();

        var r = CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_TradeDateNullOrEmpty_ThrowsMappingDateTimeException()
    {
        var ctx = MakeContext();

        var dtoNull = MakeValidCalendarOffDayAllDto() with { TradeDate = null };
        var ex1 = Assert.Throws<MappingDateTimeException>(() =>
            CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dtoNull }, ctx, NullLogger.Instance));
        Assert.Equal(MappingCategories.CalendarOffDayAll, ex1.Category);

        var dtoEmpty = MakeValidCalendarOffDayAllDto() with { TradeDate = "" };
        Assert.Throws<MappingDateTimeException>(() =>
            CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dtoEmpty }, ctx, NullLogger.Instance));
    }

    [Fact]
    public void Map_WorkdayLongFields_NullablePassThroughAsInt()
    {
        var ctx = MakeContext();

        var dto = MakeValidCalendarOffDayAllDto() with
        {
            CurrencyWorkday = 1,
            FuturesWorkday = 0,
            StockWorkday = null,
        };
        var r = CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(1, r[0].CurrencyWorkday);
        Assert.Equal(0, r[0].FuturesWorkday);
        Assert.Null(r[0].StockWorkday);
    }

    [Fact]
    public void Map_ReasonStringFields_PassThrough()
    {
        var ctx = MakeContext();
        var dto = MakeValidCalendarOffDayAllDto() with
        {
            CurrencyReason = "H",
            FuturesReason = null,
            StockReason = "T",
        };

        var r = CalendarOffDayAllMapper.MapBatch(new List<CalendarOffDaysAllDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal("H", r[0].CurrencyReason);
        Assert.Null(r[0].FuturesReason);
        Assert.Equal("T", r[0].StockReason);
    }
}
