using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestHistoryData.Phase7_5;

public class FutoiMapperTests
{
    private static MapContext CreateCtx() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "futoi",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<FutoiDTO>
        {
            new()
            {
                TradeDate = "2026-05-07",
                TradeTime = "10:05:00",
                Ticker = "Si",
                ClGroup = "FIZ",
                SessId = 1,
                SeqNum = 42,
                Pos = 1500,
                PosLong = 1000,
                PosShort = -500,
                PosLongNum = 50,
                PosShortNum = 30,
                TradeSessionDate = "2026-05-07",
            },
        };

        List<Futoi> result = FutoiMapper.MapBatch(
            dtos, "Si", CreateCtx(), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("Si", result[0].SecId);
        Assert.Equal("FIZ", result[0].ClGroup);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
    }
}
