using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;

namespace TestHistoryData.Phase8;

public class SourceDateTimeParserTests
{
    private static readonly TimeZoneInfo Msk = SourceTimezones.Resolve("Europe/Moscow");

    // ── ParseDateOnlyOrNull ───────────────────────────────────────────────

    [Fact]
    public void ParseDateOnlyOrNull_ValidDate_ReturnsDateOnly()
    {
        DateOnly? result = SourceDateTimeParser.ParseDateOnlyOrNull("2026-05-17", "test_field");
        Assert.Equal(new DateOnly(2026, 5, 17), result);
    }

    [Fact]
    public void ParseDateOnlyOrNull_Null_ReturnsNull()
    {
        DateOnly? result = SourceDateTimeParser.ParseDateOnlyOrNull(null, "test_field");
        Assert.Null(result);
    }

    [Fact]
    public void ParseDateOnlyOrNull_Empty_ReturnsNull()
    {
        DateOnly? result = SourceDateTimeParser.ParseDateOnlyOrNull("", "test_field");
        Assert.Null(result);
    }

    [Fact]
    public void ParseDateOnlyOrNull_Invalid_ThrowsMappingDateTimeException_WithFieldName()
    {
        var ex = Assert.Throws<MappingDateTimeException>(
            () => SourceDateTimeParser.ParseDateOnlyOrNull("not-a-date", "test_field"));
        Assert.Equal("test_field", ex.FieldName);
        Assert.Equal("not-a-date", ex.RawValue);
    }

    // ── ParseRequiredDateOnly ────────────────────────────────────────────

    [Fact]
    public void ParseRequiredDateOnly_ValidDate_ReturnsDateOnly()
    {
        DateOnly result = SourceDateTimeParser.ParseRequiredDateOnly("2026-05-17", "test_field");
        Assert.Equal(new DateOnly(2026, 5, 17), result);
    }

    [Fact]
    public void ParseRequiredDateOnly_Null_ThrowsMappingDateTimeException()
    {
        var ex = Assert.Throws<MappingDateTimeException>(
            () => SourceDateTimeParser.ParseRequiredDateOnly(null, "trade_date"));
        Assert.Equal("trade_date", ex.FieldName);
        Assert.Null(ex.RawValue);
    }

    // ── ParseTimeOnlyOrNull ──────────────────────────────────────────────

    [Fact]
    public void ParseTimeOnlyOrNull_ValidTime_ReturnsTimeOnly()
    {
        TimeOnly? result = SourceDateTimeParser.ParseTimeOnlyOrNull("10:00:00", "time_from");
        Assert.Equal(new TimeOnly(10, 0, 0), result);

        TimeOnly? shortResult = SourceDateTimeParser.ParseTimeOnlyOrNull("10:00", "time_from");
        Assert.Equal(new TimeOnly(10, 0, 0), shortResult);
    }

    [Fact]
    public void ParseTimeOnlyOrNull_Invalid_ThrowsMappingDateTimeException()
    {
        var ex = Assert.Throws<MappingDateTimeException>(
            () => SourceDateTimeParser.ParseTimeOnlyOrNull("25:00", "time_from"));
        Assert.Equal("time_from", ex.FieldName);
        Assert.Equal("25:00", ex.RawValue);
    }

    // ── ParseMskDateTimeToUtcOrNull ──────────────────────────────────────

    [Fact]
    public void ParseMskDateTimeToUtcOrNull_UnspecifiedKind_TreatsAsSourceTimezone()
    {
        var input = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Unspecified);
        DateTime? result = SourceDateTimeParser.ParseMskDateTimeToUtcOrNull(input, Msk, "update_time");

        Assert.NotNull(result);
        Assert.Equal(new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc), result.Value);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Fact]
    public void ParseMskDateTimeToUtcOrNull_UtcKind_RemainsUtc()
    {
        var input = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc);
        DateTime? result = SourceDateTimeParser.ParseMskDateTimeToUtcOrNull(input, Msk, "update_time");

        Assert.NotNull(result);
        Assert.Equal(input, result.Value);
        Assert.Equal(DateTimeKind.Utc, result.Value.Kind);
    }

    [Fact]
    public void ParseMskDateTimeToUtcOrNull_LocalKind_ThrowsMappingDateTimeException()
    {
        var input = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Local);
        var ex = Assert.Throws<MappingDateTimeException>(
            () => SourceDateTimeParser.ParseMskDateTimeToUtcOrNull(input, Msk, "update_time"));
        Assert.Equal("update_time", ex.FieldName);
    }

    [Fact]
    public void ParseMskDateTimeToUtcOrNull_NullInput_ReturnsNull()
    {
        DateTime? result = SourceDateTimeParser.ParseMskDateTimeToUtcOrNull(null, Msk, "update_time");
        Assert.Null(result);
    }

    // ── ParseMskDateTimeStringToUtcOrNull ────────────────────────────────

    [Fact]
    public void ParseMskDateTimeStringToUtcOrNull_Invalid_ThrowsMappingDateTimeException()
    {
        var ex = Assert.Throws<MappingDateTimeException>(
            () => SourceDateTimeParser.ParseMskDateTimeStringToUtcOrNull("invalid", Msk, "update_time"));
        Assert.Equal("update_time", ex.FieldName);
        Assert.Equal("invalid", ex.RawValue);
    }

    [Fact]
    public void ParseMskDateTimeStringToUtcOrNull_FractionalSeconds_Parses()
    {
        DateTime? short1 = SourceDateTimeParser.ParseMskDateTimeStringToUtcOrNull(
            "2026-05-17 12:00:00.1", Msk, "update_time");
        Assert.NotNull(short1);
        Assert.Equal(DateTimeKind.Utc, short1.Value.Kind);

        DateTime? long7 = SourceDateTimeParser.ParseMskDateTimeStringToUtcOrNull(
            "2026-05-17 12:00:00.1234567", Msk, "update_time");
        Assert.NotNull(long7);
        Assert.Equal(DateTimeKind.Utc, long7.Value.Kind);
    }
}
