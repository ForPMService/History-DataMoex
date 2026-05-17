using History_DataMoex.Mappers;

namespace TestHistoryData.Phase7_5;

public class RowHashHelperTests
{
    [Fact]
    public void Compute_SameInput_Deterministic()
    {
        const string s = "SBER|123456789|60|MOEX_ALGOPACK|100.5|101.0||100.8|1000|100500";
        ulong h1 = RowHashHelper.Compute(s.AsSpan());
        ulong h2 = RowHashHelper.Compute(s.AsSpan());
        Assert.Equal(h1, h2);
    }

    [Fact]
    public void Compute_DifferentInput_DifferentHash()
    {
        ulong h1 = RowHashHelper.Compute("SBER|1".AsSpan());
        ulong h2 = RowHashHelper.Compute("SBER|2".AsSpan());
        Assert.NotEqual(h1, h2);
    }

    [Fact]
    public void Compute_ShortString_DoesNotThrow()
    {
        const string s = "abc|123|MOEX";
        ulong _ = RowHashHelper.Compute(s.AsSpan());
    }

    [Fact]
    public void Compute_LongString_DoesNotThrow_ManyIterations()
    {
        string longCanonical = new string('x', 2048);
        ulong reference = RowHashHelper.Compute(longCanonical.AsSpan());

        for (int i = 0; i < 10000; i++)
        {
            ulong h = RowHashHelper.Compute(longCanonical.AsSpan());
            Assert.Equal(reference, h);
        }
    }

    [Fact]
    public void Fmt_Double_G17Format()
    {
        Assert.Equal("100.5", RowHashHelper.Fmt((double?)100.5));

        double precise = 0.1 + 0.2;
        string formatted = RowHashHelper.Fmt((double?)precise);
        Assert.Equal(precise.ToString("G17", System.Globalization.CultureInfo.InvariantCulture), formatted);
    }

    [Fact]
    public void Fmt_NullDouble_EmptyString()
    {
        Assert.Equal("", RowHashHelper.Fmt((double?)null));
    }

    [Fact]
    public void Fmt_NullInt_EmptyString()
    {
        Assert.Equal("", RowHashHelper.Fmt((int?)null));
    }

    [Fact]
    public void Fmt_NullLong_EmptyString()
    {
        Assert.Equal("", RowHashHelper.Fmt((long?)null));
    }

    [Fact]
    public void Fmt_NullString_EmptyString()
    {
        Assert.Equal("", RowHashHelper.Fmt((string?)null));
    }
}
