using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarFortsContractMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarFortsContractDTO> { MakeValidCalendarFortsContractDto() };
        var ctx = MakeContext();

        List<CalendarFortsContract> result = CalendarFortsContractMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarFortsContract row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal("SiM6", row.SecId);
        Assert.Equal(new DateOnly(2026, 6, 18), row.ExpirationDate);
        Assert.Equal(new TimeOnly(18, 45, 0), row.ExpirationTime);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarFortsContractMapper.MapBatch(
            new List<CalendarFortsContractDTO> { MakeValidCalendarFortsContractDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarFortsContractMapper.MapBatch(
            new List<CalendarFortsContractDTO> { MakeValidCalendarFortsContractDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarFortsContractDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarFortsContractDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarFortsContractDto();
        var ctx = MakeContext();

        var r = CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_SecIdEmpty_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarFortsContractDto(secId: "");
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarFortsContract, ex.Category);
    }

    [Fact]
    public void Map_SecIdDash_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarFortsContractDto(secId: "-");
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarFortsContract, ex.Category);
    }

    [Fact]
    public void Map_ExpirationDate_ParsesIsoString()
    {
        var dto = MakeValidCalendarFortsContractDto() with { ExpirationDate = "2026-06-18" };
        var ctx = MakeContext();

        var r = CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx, NullLogger.Instance);
        Assert.Equal(new DateOnly(2026, 6, 18), r[0].ExpirationDate);

        var dtoBad = MakeValidCalendarFortsContractDto() with { ExpirationDate = "not-a-date" };
        Assert.Throws<MappingDateTimeException>(() =>
            CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dtoBad }, ctx, NullLogger.Instance));
    }

    [Fact]
    public void Map_ExpirationTime_ParsesTimeOnly()
    {
        var dto = MakeValidCalendarFortsContractDto() with { ExpirationTime = "10:30:45" };
        var ctx = MakeContext();

        var r = CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx, NullLogger.Instance);
        Assert.Equal(new TimeOnly(10, 30, 45), r[0].ExpirationTime);
    }

    [Fact]
    public void Map_WeekendSession_PassedThrough()
    {
        var dto = MakeValidCalendarFortsContractDto() with { WeekendSession = 1 };
        var ctx = MakeContext();

        var r = CalendarFortsContractMapper.MapBatch(new List<CalendarFortsContractDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(1, r[0].WeekendSession);
    }
}
