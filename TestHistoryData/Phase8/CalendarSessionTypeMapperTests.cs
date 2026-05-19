using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarSessionTypeMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarSessionTypeDTO> { MakeValidCalendarSessionTypeDto() };
        var ctx = MakeContext();

        List<CalendarSessionType> result = CalendarSessionTypeMapper.MapBatch(dtos, "stock", ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarSessionType row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal("stock", row.Market);
        Assert.Equal("MAIN", row.Type);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarSessionTypeMapper.MapBatch(
            new List<CalendarSessionTypeDTO> { MakeValidCalendarSessionTypeDto() }, "stock", ctx, NullLogger.Instance);
        var r2 = CalendarSessionTypeMapper.MapBatch(
            new List<CalendarSessionTypeDTO> { MakeValidCalendarSessionTypeDto() }, "stock", ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarSessionTypeDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "stock", ctx1, NullLogger.Instance);
        var r2 = CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "stock", ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarSessionTypeDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "stock", ctx1, NullLogger.Instance);
        var r2 = CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "stock", ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarSessionTypeDto();
        var ctx = MakeContext();

        var r = CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "stock", ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_MarketStockAccepted()
    {
        var dto = MakeValidCalendarSessionTypeDto();
        var ctx = MakeContext();

        var r = CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "stock", ctx, NullLogger.Instance);

        Assert.Equal("stock", r[0].Market);
    }

    [Fact]
    public void Map_MarketFuturesAccepted()
    {
        var dto = MakeValidCalendarSessionTypeDto();
        var ctx = MakeContext();

        var r = CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "futures", ctx, NullLogger.Instance);

        Assert.Equal("futures", r[0].Market);
    }

    [Fact]
    public void Map_MarketInvalidString_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarSessionTypeDto();
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "FX", ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarSessionType, ex.Category);
        Assert.Contains("FX", ex.Message);
    }

    [Fact]
    public void Map_TypeEmpty_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarSessionTypeDto(type: "");
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "stock", ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarSessionType, ex.Category);
    }

    [Fact]
    public void Map_TypeTitleStringPassThrough()
    {
        var dto = MakeValidCalendarSessionTypeDto(type: "ADDITIONAL") with { Title = "Дополнительная" };
        var ctx = MakeContext();

        var r = CalendarSessionTypeMapper.MapBatch(new List<CalendarSessionTypeDTO> { dto }, "stock", ctx, NullLogger.Instance);

        Assert.Equal("ADDITIONAL", r[0].Type);
        Assert.Equal("Дополнительная", r[0].Title);
    }
}
