using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestHistoryData.Phase7_5;

public class MegaAlertsFuturesMapperTests
{
    private static MapContext CreateCtx() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "megaalerts_futures",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<MegaAlertsFuturesDTO>
        {
            new()
            {
                TradeDate = "2026-05-07",
                TradeTime = "10:05:00",
                SecId = "SiM6",
                AssetCode = "Si",
                AlertType = "oi_close_change_99_9_pctl-",
                Threshold = 100.0,
                Value = 150.0,
                Reference = "{}",
            },
        };

        List<MegaAlertsFutures> result = MegaAlertsFuturesMapper.MapBatch(
            dtos, "SiM6", CreateCtx(), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SiM6", result[0].SecId);
        Assert.Equal("Si", result[0].AssetCode);
        Assert.Equal("oi_close_change_99_9_pctl-", result[0].AlertType);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
    }
}
