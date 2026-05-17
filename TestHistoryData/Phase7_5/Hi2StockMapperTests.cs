using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestHistoryData.Phase7_5;

public class Hi2StockMapperTests
{
    private static MapContext CreateCtx() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "hi2_stock",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<Hi2AssetDTO>
        {
            new()
            {
                TradeDate = "2026-05-07",
                TradeTime = "10:05:00",
                SecId = "SBER",
                Metric = "hhi_volume",
                Value = 0.42,
                Reference = "",
            },
        };

        List<Hi2Stock> result = Hi2StockMapper.MapBatch(
            dtos, "SBER", CreateCtx(), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SBER", result[0].SecId);
        Assert.Equal("hhi_volume", result[0].Metric);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
    }
}
