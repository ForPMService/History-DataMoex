using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestHistoryData.Phase7_5;

public class OrderStatsMapperTests
{
    private static MapContext CreateCtx() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "orderstats",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<SuperCandlesOrderStats5mDTO>
        {
            new()
            {
                TradeDate = "2026-05-07",
                TradeTime = "10:05:00",
                SecId = "SBER",
                PutOrdersB = 25,
                PutOrdersS = 30,
                PutVal = 500000.0,
                CancelOrders = 10,
            },
        };

        List<OrderStats5m> result = OrderStatsMapper.MapBatch(
            dtos, "SBER", CreateCtx(), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SBER", result[0].SecId);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
        Assert.Equal(300, result[0].IntervalSeconds);
    }
}
