using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestHistoryData.Phase7_5;

public class FuturesOrderBookStatsMapperTests
{
    private static MapContext CreateCtx() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "futures_obstats",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<SuperCandlesFuturesOrderBookStats5mDTO>
        {
            new()
            {
                TradeDate = "2026-05-07",
                TradeTime = "10:05:00",
                SecId = "SiM5",
                AssetCode = "Si",
                MidPrice = 80025.0,
                MicroPrice = 80024.5,
                SpreadL1 = 0.5,
                LevelsB = 20,
                LevelsS = 20,
            },
        };

        List<FuturesOrderBookStats5m> result = FuturesOrderBookStatsMapper.MapBatch(
            dtos, "SiM5", CreateCtx(), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SiM5", result[0].SecId);
        Assert.Equal("Si", result[0].AssetCode);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
        Assert.Equal(300, result[0].IntervalSeconds);
    }
}
