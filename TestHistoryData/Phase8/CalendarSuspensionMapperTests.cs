using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarSuspensionMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarSuspendedDTO> { MakeValidCalendarSuspensionDto() };
        var ctx = MakeContext();

        List<CalendarSuspension> result = CalendarSuspensionMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarSuspension row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal("SBER", row.SecId);
        Assert.Equal(1, row.ReasonId);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarSuspensionMapper.MapBatch(
            new List<CalendarSuspendedDTO> { MakeValidCalendarSuspensionDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarSuspensionMapper.MapBatch(
            new List<CalendarSuspendedDTO> { MakeValidCalendarSuspensionDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarSuspensionDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarSuspensionDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarSuspensionDto();
        var ctx = MakeContext();

        var r = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_SecIdEmpty_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarSuspensionDto(secId: "");
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarSuspension, ex.Category);
    }

    [Fact]
    public void Map_ReasonIdValidIntString_ParsesToInt()
    {
        var dto = MakeValidCalendarSuspensionDto() with { ReasonId = "5002" };
        var ctx = MakeContext();

        var r = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(5002, r[0].ReasonId);
    }

    [Fact]
    public void Map_ReasonIdNullOrEmpty_RemainsNull()
    {
        var ctx = MakeContext();

        var dtoNull = MakeValidCalendarSuspensionDto() with { ReasonId = null };
        var r1 = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dtoNull }, ctx, NullLogger.Instance);
        Assert.Null(r1[0].ReasonId);

        var dtoEmpty = MakeValidCalendarSuspensionDto() with { ReasonId = "" };
        var r2 = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dtoEmpty }, ctx, NullLogger.Instance);
        Assert.Null(r2[0].ReasonId);
    }

    [Fact]
    public void Map_ReasonIdInvalidString_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarSuspensionDto() with { ReasonId = "not-a-number" };
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarSuspension, ex.Category);
        Assert.Equal(0, ex.RowIndex);
    }

    [Fact]
    public void Map_UpdateTimeMskToUtc_ConvertsViaSourceTz()
    {
        // 2026-05-17 12:00 MSK = 09:00 UTC (MSK = UTC+3 круглый год).
        var dto = MakeValidCalendarSuspensionDto() with
        {
            UpdateTime = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Unspecified),
        };
        var ctx = MakeContext();

        var r = CalendarSuspensionMapper.MapBatch(new List<CalendarSuspendedDTO> { dto }, ctx, NullLogger.Instance);

        Assert.NotNull(r[0].UpdateTimeUtc);
        Assert.Equal(new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc), r[0].UpdateTimeUtc!.Value);
        Assert.Equal(DateTimeKind.Utc, r[0].UpdateTimeUtc!.Value.Kind);
    }
}
