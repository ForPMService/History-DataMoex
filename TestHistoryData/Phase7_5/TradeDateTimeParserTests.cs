using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;

namespace TestHistoryData.Phase7_5;

public class TradeDateTimeParserTests
{
    [Fact]
    public void Parse_ValidStandardFormat_ReturnsUnspecifiedKind()
    {
        DateTime result = TradeDateTimeParser.Parse("2026-05-05", "10:05:00");
        Assert.Equal(new DateTime(2026, 5, 5, 10, 5, 0), result);
        Assert.Equal(DateTimeKind.Unspecified, result.Kind);
    }

    [Fact]
    public void Parse_ValidFormatWithMilliseconds_Works()
    {
        DateTime result = TradeDateTimeParser.Parse("2026-05-05", "10:05:00.123");
        Assert.Equal(new DateTime(2026, 5, 5, 10, 5, 0).AddMilliseconds(123), result);
    }

    [Fact]
    public void Parse_NoLeadingZero_Works()
    {
        DateTime result = TradeDateTimeParser.Parse("2026-05-05", "9:05:00");
        Assert.Equal(new DateTime(2026, 5, 5, 9, 5, 0), result);
    }

    [Fact]
    public void Parse_NullDate_ThrowsMappingDateTimeException()
    {
        var ex = Assert.Throws<MappingDateTimeException>(
            () => TradeDateTimeParser.Parse(null, "10:05:00"));
        Assert.Null(ex.RawTradeDate);
        Assert.Equal("10:05:00", ex.RawTradeTime);
    }

    [Fact]
    public void Parse_NullTime_ThrowsMappingDateTimeException()
    {
        var ex = Assert.Throws<MappingDateTimeException>(
            () => TradeDateTimeParser.Parse("2026-05-05", null));
        Assert.Equal("2026-05-05", ex.RawTradeDate);
        Assert.Null(ex.RawTradeTime);
    }

    [Fact]
    public void Parse_InvalidFormat_ThrowsMappingDateTimeException()
    {
        var ex = Assert.Throws<MappingDateTimeException>(
            () => TradeDateTimeParser.Parse("05-2026-05", "10:05:00"));
        Assert.Equal("05-2026-05", ex.RawTradeDate);
        Assert.Equal("10:05:00", ex.RawTradeTime);
    }

    [Fact]
    public void ParseToUtc_MskToUtc_ConvertsCorrectly()
    {
        TimeZoneInfo msk = SourceTimezones.Resolve("Europe/Moscow");
        (DateTime utc, DateTime local) = TradeDateTimeParser.ParseToUtc(
            "2026-05-05", "10:05:00", msk);

        Assert.Equal(new DateTime(2026, 5, 5, 7, 5, 0, DateTimeKind.Utc), utc);
        Assert.Equal(DateTimeKind.Utc, utc.Kind);
        Assert.Equal(DateTimeKind.Unspecified, local.Kind);
        Assert.Equal(new DateTime(2026, 5, 5, 10, 5, 0), local);
    }
}
