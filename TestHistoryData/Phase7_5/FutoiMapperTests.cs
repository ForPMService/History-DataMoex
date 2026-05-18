using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;
using History_DataMoex.Models;
using Microsoft.Extensions.Logging.Abstractions;
using static TestHistoryData.Phase7_5.MapperTestsHelpers;

namespace TestHistoryData.Phase7_5;

public class FutoiMapperTests
{
    [Fact]
    public void MapBatch_DefaultDtoList_DoesNotThrow()
    {
        var dtos = new List<FutoiDTO>
        {
            MakeValidFutoiDto("2026-05-07", "10:05:00", "Si"),
        };

        List<Futoi> result = FutoiMapper.MapBatch(
            dtos, "Si", MakeContext("MOEX_ALGOPACK", "Europe/Moscow"), NullLogger.Instance);

        Assert.Single(result);
        Assert.Equal("Si", result[0].SecId);
        Assert.Equal("FIZ", result[0].ClGroup);
        Assert.Equal(DateTimeKind.Utc, result[0].BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, result[0].BeginLocal.Kind);
    }

    [Fact]
    public void Map_TimezoneConversion()
    {
        var dto = MakeValidFutoiDto("2026-05-07", "10:05:00", "Si");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        Futoi model = FutoiMapper.Map(dto, "Si", ctx, tz);

        Assert.Equal(new DateTime(2026, 5, 7, 7, 5, 0, DateTimeKind.Utc), model.BeginUtc);
        Assert.Equal(new DateTime(2026, 5, 7, 10, 5, 0), model.BeginLocal);
        Assert.Equal(DateTimeKind.Utc, model.BeginUtc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, model.BeginLocal.Kind);
    }

    [Fact]
    public void Map_HashStability()
    {
        var dto = MakeValidFutoiDto("2026-05-07", "10:05:00", "Si");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        Futoi m1 = FutoiMapper.Map(dto, "Si", ctx, tz);
        Futoi m2 = FutoiMapper.Map(dto, "Si", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_DifferentSource_DifferentHash()
    {
        var dto = MakeValidFutoiDto("2026-05-07", "10:05:00", "Si");
        var ctxAlg = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctxIss = ctxAlg with { SourceCode = "MOEX_ISS" };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctxAlg.SourceTimezone);

        Futoi m1 = FutoiMapper.Map(dto, "Si", ctxAlg, tz);
        Futoi m2 = FutoiMapper.Map(dto, "Si", ctxIss, tz);

        Assert.NotEqual(m1.RowHashV1, m2.RowHashV1);
    }

    [Fact]
    public void Map_NullFields_HashStable()
    {
        var dto = MakeMinimalFutoiDto("2026-05-07", "10:05:00", "Si");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        Futoi m1 = FutoiMapper.Map(dto, "Si", ctx, tz);
        Futoi m2 = FutoiMapper.Map(dto, "Si", ctx, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(0UL, m1.RowHashV1);
    }

    [Fact]
    public void Map_DifferentLoadJobId_SameHash()
    {
        var dto = MakeValidFutoiDto("2026-05-07", "10:05:00", "Si");
        var ctx1 = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        var ctx2 = ctx1 with
        {
            RawObjectId = Guid.CreateVersion7(),
            LoadJobId = Guid.CreateVersion7(),
        };
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx1.SourceTimezone);

        Futoi m1 = FutoiMapper.Map(dto, "Si", ctx1, tz);
        Futoi m2 = FutoiMapper.Map(dto, "Si", ctx2, tz);

        Assert.Equal(m1.RowHashV1, m2.RowHashV1);
        Assert.NotEqual(m1.RawObjectId, m2.RawObjectId);
        Assert.NotEqual(m1.LoadJobId, m2.LoadJobId);
    }

    // ── FUTOI-specific (3 теста) ─────────────────────────────────────────

    [Fact]
    public void Map_TickerMatchesParam_OK()
    {
        var dto = MakeValidFutoiDto("2026-05-07", "10:05:00", "Si");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        Futoi model = FutoiMapper.Map(dto, "Si", ctx, tz);

        Assert.Equal("Si", model.SecId);
    }

    [Fact]
    public void Map_TickerMismatch_ThrowsMappingValidationException()
    {
        var dto = MakeValidFutoiDto("2026-05-07", "10:05:00", "Si");
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        var ex = Assert.Throws<MappingValidationException>(() =>
            FutoiMapper.Map(dto, "BR", ctx, tz, rowIndex: 7));

        Assert.Equal(MappingCategories.Futoi, ex.Category);
        Assert.Equal("BR", ex.SecId);
        Assert.Equal(7, ex.RowIndex);
        Assert.Contains("Si", ex.Message);
    }

    [Fact]
    public void Map_DifferentClGroup_DifferentHash()
    {
        var dtoFiz = MakeValidFutoiDto("2026-05-07", "10:05:00", "Si", clGroup: "FIZ");
        var dtoYur = dtoFiz with { ClGroup = "YUR" };
        var ctx = MakeContext("MOEX_ALGOPACK", "Europe/Moscow");
        TimeZoneInfo tz = SourceTimezones.Resolve(ctx.SourceTimezone);

        Futoi m1 = FutoiMapper.Map(dtoFiz, "Si", ctx, tz);
        Futoi m2 = FutoiMapper.Map(dtoYur, "Si", ctx, tz);

        Assert.NotEqual(m1.RowHashV1, m2.RowHashV1);
    }
}
