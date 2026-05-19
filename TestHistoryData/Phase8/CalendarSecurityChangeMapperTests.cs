using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarSecurityChangeMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarSecurityChangeDTO> { MakeValidCalendarSecurityChangeDto() };
        var ctx = MakeContext();

        List<CalendarSecurityChange> result = CalendarSecurityChangeMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarSecurityChange row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal("SBER", row.SecId);
        Assert.Equal(DateTimeKind.Utc, row.UpdateTimeUtc.Kind);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarSecurityChangeMapper.MapBatch(
            new List<CalendarSecurityChangeDTO> { MakeValidCalendarSecurityChangeDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarSecurityChangeMapper.MapBatch(
            new List<CalendarSecurityChangeDTO> { MakeValidCalendarSecurityChangeDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarSecurityChangeDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarSecurityChangeMapper.MapBatch(new List<CalendarSecurityChangeDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarSecurityChangeMapper.MapBatch(new List<CalendarSecurityChangeDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarSecurityChangeDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarSecurityChangeMapper.MapBatch(new List<CalendarSecurityChangeDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarSecurityChangeMapper.MapBatch(new List<CalendarSecurityChangeDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarSecurityChangeDto();
        var ctx = MakeContext();

        var r = CalendarSecurityChangeMapper.MapBatch(new List<CalendarSecurityChangeDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_SecIdEmpty_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarSecurityChangeDto(secId: "");
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarSecurityChangeMapper.MapBatch(new List<CalendarSecurityChangeDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarSecurityChange, ex.Category);
    }

    [Fact]
    public void Map_UpdateTimeMskToUtc_ConvertsViaSourceTz()
    {
        var dto = MakeValidCalendarSecurityChangeDto() with
        {
            UpdateTime = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Unspecified),
        };
        var ctx = MakeContext();

        var r = CalendarSecurityChangeMapper.MapBatch(new List<CalendarSecurityChangeDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc), r[0].UpdateTimeUtc);
        Assert.Equal(DateTimeKind.Utc, r[0].UpdateTimeUtc.Kind);
    }

    [Fact]
    public void Map_BeforeValueAfterValue_StringPassThrough()
    {
        var dto = MakeValidCalendarSecurityChangeDto() with { BeforeValue = "10", AfterValue = "1" };
        var ctx = MakeContext();

        var r = CalendarSecurityChangeMapper.MapBatch(new List<CalendarSecurityChangeDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal("10", r[0].BeforeValue);
        Assert.Equal("1", r[0].AfterValue);
    }
}
