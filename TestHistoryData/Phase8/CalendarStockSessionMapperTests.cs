using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class CalendarStockSessionMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<CalendarStockSessionDTO> { MakeValidCalendarStockSessionDto() };
        var ctx = MakeContext();

        List<CalendarStockSession> result = CalendarStockSessionMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        CalendarStockSession row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        // Default фабрика передаёт "-" → normalize в null (Group B).
        Assert.Null(row.SecId);
        Assert.Equal(new DateOnly(2026, 5, 17), row.TradeDate);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var ctx = MakeContext();
        var r1 = CalendarStockSessionMapper.MapBatch(
            new List<CalendarStockSessionDTO> { MakeValidCalendarStockSessionDto() }, ctx, NullLogger.Instance);
        var r2 = CalendarStockSessionMapper.MapBatch(
            new List<CalendarStockSessionDTO> { MakeValidCalendarStockSessionDto() }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidCalendarStockSessionDto();
        var ctx1 = MakeContext("MOEX_CALENDAR");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.NotEqual(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidCalendarStockSessionDto();
        var ctx1 = MakeContext();
        var ctx2 = MakeContext();

        var r1 = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidCalendarStockSessionDto();
        var ctx = MakeContext();

        var r = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_SecIdEmpty_NormalizesToNull()
    {
        var dto = MakeValidCalendarStockSessionDto(secId: "");
        var ctx = MakeContext();

        var r = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Null(r[0].SecId);
    }

    [Fact]
    public void Map_SecIdDash_NormalizesToNull()
    {
        var dto = MakeValidCalendarStockSessionDto(secId: "-");
        var ctx = MakeContext();

        var r = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Null(r[0].SecId);
    }

    [Fact]
    public void Map_SecIdValid_PassedThrough()
    {
        var dto = MakeValidCalendarStockSessionDto(secId: "SBER");
        var ctx = MakeContext();

        var r = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal("SBER", r[0].SecId);
    }

    [Fact]
    public void Map_TradingSessionNegative_PreservedAsInt()
    {
        var dto = MakeValidCalendarStockSessionDto() with { TradingSession = -999 };
        var ctx = MakeContext();

        var r = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(-999, r[0].TradingSession);
    }

    [Fact]
    public void Map_UpdateTimeMskToUtc_ConvertsViaSourceTz()
    {
        var dto = MakeValidCalendarStockSessionDto() with
        {
            UpdateTime = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Unspecified),
        };
        var ctx = MakeContext();

        var r = CalendarStockSessionMapper.MapBatch(new List<CalendarStockSessionDTO> { dto }, ctx, NullLogger.Instance);

        Assert.NotNull(r[0].UpdateTimeUtc);
        Assert.Equal(new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc), r[0].UpdateTimeUtc!.Value);
    }
}
