using System.Globalization;
using History_DataMoex.Mappers;

namespace TestHistoryData.Phase8;

public class RowHashHelperPhase8Tests
{
    [Fact]
    public void Fmt_Decimal_G29Format_NormalizesScale()
    {
        // Critical: frozen V1 контракт — 1.0m и 1.00m обязаны давать одну строку.
        string a = RowHashHelper.Fmt((decimal?)1.0m);
        string b = RowHashHelper.Fmt((decimal?)1.00m);
        Assert.Equal(a, b);
        Assert.Equal("1", a);
    }

    [Fact]
    public void Fmt_NullDecimal_EmptyString()
    {
        Assert.Equal("", RowHashHelper.Fmt((decimal?)null));
    }

    [Fact]
    public void Fmt_DateOnly_FormatsAsYyyyMMdd()
    {
        Assert.Equal("2026-05-17", RowHashHelper.Fmt((DateOnly?)new DateOnly(2026, 5, 17)));
    }

    [Fact]
    public void Fmt_NullDateOnly_EmptyString()
    {
        Assert.Equal("", RowHashHelper.Fmt((DateOnly?)null));
    }

    [Fact]
    public void Fmt_TimeOnly_FormatsAsHHmmss()
    {
        Assert.Equal("10:05:30", RowHashHelper.Fmt((TimeOnly?)new TimeOnly(10, 5, 30)));
        Assert.Equal("00:00:00", RowHashHelper.Fmt((TimeOnly?)new TimeOnly(0, 0, 0)));
    }

    [Fact]
    public void FmtUtc_DateTimeUtc_FormatsAsTicks()
    {
        var dt = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc);
        string expected = dt.Ticks.ToString(CultureInfo.InvariantCulture);
        Assert.Equal(expected, RowHashHelper.FmtUtc(dt));
    }

    [Fact]
    public void FmtUtc_Null_EmptyString()
    {
        Assert.Equal("", RowHashHelper.FmtUtc(null));
    }
}
