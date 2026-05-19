using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarSecurityAttributeMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarSecurityAttributeDTO> { MakeValidCalendarSecurityAttributeDto() };
        var ctx = MakeContext();

        List<CalendarSecurityAttribute> result = CalendarSecurityAttributeMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarSecurityAttribute row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal("LOTSIZE", row.Name);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarSecurityAttributeMapper.MapBatch(
            new List<CalendarSecurityAttributeDTO> { MakeValidCalendarSecurityAttributeDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarSecurityAttributeMapper.MapBatch(
            new List<CalendarSecurityAttributeDTO> { MakeValidCalendarSecurityAttributeDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarSecurityAttributeDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarSecurityAttributeMapper.MapBatch(new List<CalendarSecurityAttributeDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarSecurityAttributeMapper.MapBatch(new List<CalendarSecurityAttributeDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarSecurityAttributeDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarSecurityAttributeMapper.MapBatch(new List<CalendarSecurityAttributeDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarSecurityAttributeMapper.MapBatch(new List<CalendarSecurityAttributeDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarSecurityAttributeDto();
        var ctx = MakeContext();

        var r = CalendarSecurityAttributeMapper.MapBatch(new List<CalendarSecurityAttributeDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_NameEmpty_ThrowsMappingValidationException()
    {
        var dto = MakeValidCalendarSecurityAttributeDto(name: "");
        var ctx = MakeContext();

        var ex = Assert.Throws<MappingValidationException>(() =>
            CalendarSecurityAttributeMapper.MapBatch(new List<CalendarSecurityAttributeDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.CalendarSecurityAttribute, ex.Category);
    }

    [Fact]
    public void Map_NameTypeTitle_StringPassThrough()
    {
        var dto = MakeValidCalendarSecurityAttributeDto() with
        {
            Name = "COUPONDATE",
            Type = "D",
            Title = "Дата выплаты купона",
        };
        var ctx = MakeContext();

        var r = CalendarSecurityAttributeMapper.MapBatch(new List<CalendarSecurityAttributeDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal("COUPONDATE", r[0].Name);
        Assert.Equal("D", r[0].Type);
        Assert.Equal("Дата выплаты купона", r[0].Title);
    }
}
