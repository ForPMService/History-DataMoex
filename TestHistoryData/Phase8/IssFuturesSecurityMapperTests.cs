using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Mappers;
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
}
