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
}
