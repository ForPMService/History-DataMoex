using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase7_5.MapperTestsHelpers;

namespace TestHistoryData.Phase7_5;

public class MegaAlertsFuturesMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<MegaAlertsFuturesDTO>
        {
            MakeValidMegaAlertsFuturesDto("2026-05-07", "10:05:00", "SiM6"),
        };

        List<MegaAlertsFutures> result = MegaAlertsFuturesMapper.MapBatch(
            dtos, "SiM6", MakeContext("MOEX_ALGOPACK", "Europe/Moscow"), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("SiM6", result[0].SecId);
        Assert.Equal("Si", result[0].AssetCode);
        Assert.Equal("oi_close_change_99_9_pctl-", result[0].AlertType);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
    }

    [Fact]
    public void Map_TimezoneConversion()
    {
        var dto = MakeValidMegaAlertsFuturesDto("2026-05-07", "10:05:00", "SiM6");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        MegaAlertsFutures model = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctx, tz);

        Assert.Equal(new DateTime(2026, 5, 7, 7, 5, 0, DateTimeKind.Utc), model.BeginUtc);
        Assert.Equal(new DateTime(2026, 5, 7, 10, 5, 0), model.BeginLocal);
        Assert.Equal(DateTimeKind.Utc, model.BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, model.BeginLocal.Kind);
    }

    [Fact]
    public void Map_HashStability()
    {
        var dto = MakeValidMegaAlertsFuturesDto("2026-05-07", "10:05:00", "SiM6");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        MegaAlertsFutures m1 = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctx, tz);
        MegaAlertsFutures m2 = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidMegaAlertsFuturesDto("2026-05-07", "10:05:00", "SiM6");
        var ctxAlg = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctxIss = ctxAlg with { SourceCode = "MOEX_ISS" };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctxAlg.SourceTimezone);

        MegaAlertsFutures m1 = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctxAlg, tz);
        MegaAlertsFutures m2 = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctxIss, tz);

        Assert.NotEqual(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_NullFields_HashStable()
    {
        var dto = MakeMinimalMegaAlertsFuturesDto("2026-05-07", "10:05:00", "SiM6");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        MegaAlertsFutures m1 = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctx, tz);
        MegaAlertsFutures m2 = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(0UL, m1.RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidMegaAlertsFuturesDto("2026-05-07", "10:05:00", "SiM6");
        var ctx1 = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctx2 = ctx1 with
        {
            RawObjectId = Guid.CreateVersion7(),
            LoadJobId = Guid.CreateVersion7(),
        };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx1.SourceTimezone);

        MegaAlertsFutures m1 = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctx1, tz);
        MegaAlertsFutures m2 = MegaAlertsFuturesMapper.Map(dto, "SiM6", ctx2, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(m1.RawObjectId, m2.RawObjectId);
        Assert.NotEqual(m1.LoadJobId, m2.LoadJobId);
    }

    [Fact]
    public void Map_DtoSecIdMismatch_ThrowsMappingValidationException()
    {
        var dto = MakeValidMegaAlertsFuturesDto("2026-05-07", "10:05:00", "BRM6");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        var ex = Assert.Throws<MappingValidationException>(() =>
            MegaAlertsFuturesMapper.Map(dto, "SiM6", ctx, tz, rowIndex: 21));

        Assert.Equal("mapping_validation", ex.ErrorCategory);
        Assert.Equal(MappingCategories.MegaAlertsFutures, ex.Category);
        Assert.Equal("SiM6", ex.SecId);
        Assert.Equal(21, ex.RowIndex);
    }
}
