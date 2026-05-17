using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Models;

namespace TestHistoryData.Phase7;

public class CandlesMapperTests
{
    private static MapContext CreateTestContext(string sourceCode = "MOEX_ALGOPACK")
        => new(
            SourceCode: sourceCode,
            Endpoint: "/datashop/algopack/eq/candles/SBER.json",
            SourceTimezone: "Europe/Moscow",
            LoadJobId: Guid.CreateVersion7(),
            RawObjectId: Guid.CreateVersion7(),
            FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    private static readonly TimeZoneInfo MoscowTz =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Moscow");

    [Fact]
    public void Map_MskToUtc_ConvertsCorrectly()
    {
        var dto = new CandlesDTO { Begin = new DateTime(2026, 5, 7, 10, 0, 0) };
        var ctx = CreateTestContext();

        Candle1m candle = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        DateTime expectedUtc = new DateTime(2026, 5, 7, 7, 0, 0, DateTimeKind.Utc);
        Assert.Equal(expectedUtc, candle.BeginUtc);
        Assert.Equal(DateTimeKind.Utc, candle.BeginUtc.Kind);
    }

    [Fact]
    public void Map_BeginLocal_PreservesOriginal()
    {
        var dto = new CandlesDTO { Begin = new DateTime(2026, 5, 7, 10, 0, 0) };
        var ctx = CreateTestContext();

        Candle1m candle = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        Assert.Equal(dto.Begin, candle.BeginLocal);
    }

    [Fact]
    public void Map_SameDto_SameHash()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = 300.5, High = 301.0, Low = 299.0, Close = 300.8,
            Volume = 1000, Value = 300500
        };
        var ctx = CreateTestContext();

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        Assert.Equal(candle1.RowHashV1, candle2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = 300.5, High = 301.0, Low = 299.0, Close = 300.8
        };
        var ctx1 = CreateTestContext("MOEX_ALGOPACK");
        var ctx2 = CreateTestContext("MOEX_ISS");

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx1, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "SBER", ctx2, MoscowTz);

        Assert.NotEqual(candle1.RowHashV1, candle2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSecId_DifferentHash()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = 300.5, High = 301.0, Low = 299.0, Close = 300.8
        };
        var ctx = CreateTestContext();

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "GAZP", ctx, MoscowTz);

        Assert.NotEqual(candle1.RowHashV1, candle2.RowHashV1);
    }

    [Fact]
    public void Map_NullBegin_Throws()
    {
        var dto = new CandlesDTO { Begin = null };
        var ctx = CreateTestContext();

        Assert.Throws<InvalidOperationException>(
            () => CandlesMapper.Map(dto, "SBER", ctx, MoscowTz));
    }

    [Fact]
    public void Map_NullPrice_HashStable()
    {
        var dto = new CandlesDTO
        {
            Begin = new DateTime(2026, 5, 7, 10, 0, 0),
            Open = null, High = 301.0, Low = 299.0, Close = 300.8
        };
        var ctx = CreateTestContext();

        Candle1m candle1 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);
        Candle1m candle2 = CandlesMapper.Map(dto, "SBER", ctx, MoscowTz);

        Assert.Equal(candle1.RowHashV1, candle2.RowHashV1);
    }
}
