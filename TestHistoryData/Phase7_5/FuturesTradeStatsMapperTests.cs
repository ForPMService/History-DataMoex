using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestHistoryData.Phase7_5;

public class FuturesTradeStatsMapperTests
{
    private static MapContext CreateCtx() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "futures_tradestats",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<SuperCandlesFuturesTradeStats5mDTO>
        {
            new()
            {
                TradeDate = "2026-05-07",
                TradeTime = "10:05:00",
                SecId = "SiM5",
                AssetCode = "Si",
                PrOpen = 80000.0,
                PrClose = 80050.0,
                Vol = 250,
                OiClose = 1000,
            },
        };

        List<FuturesTradeStats5m> result = FuturesTradeStatsMapper.MapBatch(
            dtos, "SiM5", CreateCtx(), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SiM5", result[0].SecId);
        Assert.Equal("Si", result[0].AssetCode);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
        Assert.Equal(300, result[0].IntervalSeconds);
    }
}
