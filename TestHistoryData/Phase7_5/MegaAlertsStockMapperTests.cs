using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestHistoryData.Phase7_5;

public class MegaAlertsStockMapperTests
{
    private static MapContext CreateCtx() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "megaalerts_stock",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<MegaAlertsAssetsDTO>
        {
            new()
            {
                TradeDate = "2026-05-07",
                TradeTime = "10:05:00",
                SecId = "SBER",
                AlertType = "vol_99_9_pctl",
                Threshold = 100000.0,
                Value = 250000.0,
                Reference = "{}",
            },
        };

        List<MegaAlertsStock> result = MegaAlertsStockMapper.MapBatch(
            dtos, "SBER", CreateCtx(), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SBER", result[0].SecId);
        Assert.Equal("vol_99_9_pctl", result[0].AlertType);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
    }
}
