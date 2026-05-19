using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class IssFuturesSecurityMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<FuturesSecurityDTO> { MakeValidIssFuturesSecurityDto() };
        var ctx = MakeContext("MOEX_ISS");

        List<IssFuturesSecurity> result = IssFuturesSecurityMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        IssFuturesSecurity row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal("SiM6", row.SecId);
        Assert.Equal(5000m, row.InitialMargin);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var dto1 = MakeValidIssFuturesSecurityDto();
        var dto2 = MakeValidIssFuturesSecurityDto();
        var ctx = MakeContext("MOEX_ISS");

        var r1 = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto1 }, ctx, NullLogger.Instance);
        var r2 = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto2 }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidIssFuturesSecurityDto();
        var ctxIss = MakeContext("MOEX_ISS");
        var ctxOther = MakeContext("MOEX_ALGOPACK");

        var rIss = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctxIss, NullLogger.Instance);
        var rAlg = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctxOther, NullLogger.Instance);

        Assert.NotEqual(rIss[0].RowHashV1, rAlg[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidIssFuturesSecurityDto();
        var ctx1 = MakeContext("MOEX_ISS");
        var ctx2 = MakeContext("MOEX_ISS");

        var r1 = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctx2, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidIssFuturesSecurityDto();
        var ctx = MakeContext("MOEX_ISS");

        var r = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(ctx.SourceCode, r[0].Source);
        Assert.Equal(ctx.RawObjectId, r[0].RawObjectId);
        Assert.Equal(ctx.LoadJobId, r[0].LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, r[0].FetchedAtUtc);
        Assert.NotEqual(0UL, r[0].RowHashV1);
    }

    [Fact]
    public void Map_SecIdEmpty_ThrowsMappingValidationException()
    {
        var dto = MakeValidIssFuturesSecurityDto(secId: "");
        var ctx = MakeContext("MOEX_ISS");

        var ex = Assert.Throws<MappingValidationException>(() =>
            IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.IssFuturesSecurity, ex.Category);
    }

    [Fact]
    public void Map_SevenDoubleDecimalCasts_AllExplicitCast()
    {
        var dto = MakeValidIssFuturesSecurityDto() with
        {
            INITIALMARGIN = 5000.5,
            PREVSETTLEPRICE = 92450.25,
            MINSTEP = 0.5,
            HIGHLIMIT = 95000.75,
            LOWLIMIT = 90000.125,
            STEPPRICE = 1.5,
            PREVPRICE = 92500.6,
        };
        var ctx = MakeContext("MOEX_ISS");

        var r = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctx, NullLogger.Instance);
        IssFuturesSecurity row = r[0];

        Assert.Equal(5000.5m, row.InitialMargin);
        Assert.Equal(92450.25m, row.PrevSettlePrice);
        Assert.Equal(0.5m, row.MinStep);
        Assert.Equal(95000.75m, row.HighLimit);
        Assert.Equal(90000.125m, row.LowLimit);
        Assert.Equal(1.5m, row.StepPrice);
        Assert.Equal(92500.6m, row.PrevPrice);
    }

    [Fact]
    public void Map_LastTradeDateLastDelDate_DateOnlyConversion()
    {
        var dto = MakeValidIssFuturesSecurityDto() with
        {
            LASTTRADEDATE = new DateTime(2026, 6, 18),
            LASTDELDATE = new DateTime(2026, 6, 19),
        };
        var ctx = MakeContext("MOEX_ISS");

        var r = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(new DateOnly(2026, 6, 18), r[0].LastTradeDate);
        Assert.Equal(new DateOnly(2026, 6, 19), r[0].LastDelDate);
    }

    [Fact]
    public void Map_NullableLong_PrevOpenPosition_PassThrough()
    {
        var dto = MakeValidIssFuturesSecurityDto() with { PREVOPENPOSITION = 1234567L };
        var ctx = MakeContext("MOEX_ISS");

        var r = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(1234567L, r[0].PrevOpenPosition);

        var dtoNull = MakeValidIssFuturesSecurityDto() with { PREVOPENPOSITION = null };
        var rNull = IssFuturesSecurityMapper.MapBatch(new List<FuturesSecurityDTO> { dtoNull }, ctx, NullLogger.Instance);
        Assert.Null(rNull[0].PrevOpenPosition);
    }
}
