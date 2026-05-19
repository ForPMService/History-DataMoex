using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase8.Phase8MapperTestsHelpers;

namespace TestHistoryData.Phase8;

public class IssStockSecurityMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<StockSecurityDTO> { MakeValidIssStockSecurityDto() };
        var ctx = MakeContext("MOEX_ISS");

        List<IssStockSecurity> result = IssStockSecurityMapper.MapBatch(dtos, ctx, NullLogger.Instance);

        Assert.Single(result);
        IssStockSecurity row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);

        Assert.NotEqual(0UL, row.RowHashV1);
        Assert.Equal("SBER", row.SecId);
        Assert.Equal("TQBR", row.BoardId);
    }

    [Fact]
    public void Map_HashStability_IdenticalDtosProduceSameHash()
    {
        var dto1 = MakeValidIssStockSecurityDto();
        var dto2 = MakeValidIssStockSecurityDto();
        var ctx = MakeContext("MOEX_ISS");

        var r1 = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto1 }, ctx, NullLogger.Instance);
        var r2 = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto2 }, ctx, NullLogger.Instance);

        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidIssStockSecurityDto();
        var ctxIss = MakeContext("MOEX_ISS");
        var ctxCalendar = MakeContext("MOEX_CALENDAR");

        var rIss = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctxIss, NullLogger.Instance);
        var rCal = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctxCalendar, NullLogger.Instance);

        Assert.NotEqual(rIss[0].RowHashV1, rCal[0].RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidIssStockSecurityDto();
        var ctx1 = MakeContext("MOEX_ISS");
        var ctx2 = MakeContext("MOEX_ISS");

        Assert.NotEqual(ctx1.LoadJobId, ctx2.LoadJobId);
        Assert.NotEqual(ctx1.RawObjectId, ctx2.RawObjectId);

        var r1 = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctx1, NullLogger.Instance);
        var r2 = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctx2, NullLogger.Instance);

        // Lineage поля НЕ входят в canonical (V1 frozen).
        Assert.Equal(r1[0].RowHashV1, r2[0].RowHashV1);
    }

    [Fact]
    public void Map_LineagePropagation_AllFiveCommonFieldsFromContext()
    {
        var dto = MakeValidIssStockSecurityDto();
        var ctx = MakeContext("MOEX_ISS");

        var result = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctx, NullLogger.Instance);
        IssStockSecurity row = result[0];

        Assert.Equal(ctx.SourceCode, row.Source);
        Assert.Equal(ctx.RawObjectId, row.RawObjectId);
        Assert.Equal(ctx.LoadJobId, row.LoadJobId);
        Assert.Equal(ctx.FetchedAtUtc, row.FetchedAtUtc);
        Assert.NotEqual(0UL, row.RowHashV1);
    }

    [Fact]
    public void Map_SecIdEmpty_ThrowsMappingValidationException()
    {
        var dto = MakeValidIssStockSecurityDto(secId: "");
        var ctx = MakeContext("MOEX_ISS");

        var ex = Assert.Throws<MappingValidationException>(() =>
            IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.IssStockSecurity, ex.Category);
    }

    [Fact]
    public void Map_BoardIdEmpty_ThrowsMappingValidationException()
    {
        var dto = MakeValidIssStockSecurityDto(boardId: "");
        var ctx = MakeContext("MOEX_ISS");

        var ex = Assert.Throws<MappingValidationException>(() =>
            IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctx, NullLogger.Instance));

        Assert.Equal(MappingCategories.IssStockSecurity, ex.Category);
        Assert.Equal("SBER", ex.SecId);
    }

    [Fact]
    public void Map_DoubleFaceValue_CastsToDecimalExplicit()
    {
        var dto = MakeValidIssStockSecurityDto() with { FACEVALUE = 3.14 };
        var ctx = MakeContext("MOEX_ISS");

        var result = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(3.14m, result[0].FaceValue);

        var dtoNull = MakeValidIssStockSecurityDto() with { FACEVALUE = null };
        var rNull = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dtoNull }, ctx, NullLogger.Instance);
        Assert.Null(rNull[0].FaceValue);
    }

    [Fact]
    public void Map_PrevDateDateTimeToDateOnly_Converts()
    {
        var dto = MakeValidIssStockSecurityDto() with { PREVDATE = new DateTime(2026, 5, 17) };
        var ctx = MakeContext("MOEX_ISS");

        var result = IssStockSecurityMapper.MapBatch(new List<StockSecurityDTO> { dto }, ctx, NullLogger.Instance);

        Assert.Equal(new DateOnly(2026, 5, 17), result[0].PrevDate);
    }
}
