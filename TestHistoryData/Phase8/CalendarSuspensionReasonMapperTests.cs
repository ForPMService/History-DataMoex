using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarSuspensionReasonMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarSuspendedReasonDTO> { MakeValidCalendarSuspensionReasonDto() };
        var ctx = MakeContext();

        List<CalendarSuspensionReason> result = CalendarSuspensionReasonMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarSuspensionReason row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal(1, row.Id);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarSuspensionReasonMapper.MapBatch(
            new List<CalendarSuspendedReasonDTO> { MakeValidCalendarSuspensionReasonDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarSuspensionReasonMapper.MapBatch(
            new List<CalendarSuspendedReasonDTO> { MakeValidCalendarSuspensionReasonDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarSuspensionReasonDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarSuspensionReasonMapper.MapBatch(new List<CalendarSuspendedReasonDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarSuspensionReasonMapper.MapBatch(new List<CalendarSuspendedReasonDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarSuspensionReasonDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarSuspensionReasonMapper.MapBatch(new List<CalendarSuspendedReasonDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarSuspensionReasonMapper.MapBatch(new List<CalendarSuspendedReasonDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarSuspensionReasonDto();
        var ctx = MakeContext();

        var r = CalendarSuspensionReasonMapper.MapBatch(new List<CalendarSuspendedReasonDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_IdNull_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarSuspensionReasonDto() with { Id = null };
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarSuspensionReasonMapper.MapBatch(new List<CalendarSuspendedReasonDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarSuspensionReason, ex.Category);
    }

    [Fact]
    public void Map_IdIntPreservedAsInt()
    {
        var dto = MakeValidCalendarSuspensionReasonDto(id: 5002);
        var ctx = MakeContext();

        var r = CalendarSuspensionReasonMapper.MapBatch(new List<CalendarSuspendedReasonDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(5002, r[0].Id);
    }

    [Fact]
    public void Map_TitleStringPassThrough()
    {
        var dto = MakeValidCalendarSuspensionReasonDto() with { Title = "Manual halt" };
        var ctx = MakeContext();

        var r = CalendarSuspensionReasonMapper.MapBatch(new List<CalendarSuspendedReasonDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal("Manual halt", r[0].Title);
    }
}
