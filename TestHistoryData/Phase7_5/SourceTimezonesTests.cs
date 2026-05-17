using History_DataMoex.Mappers;
using History_DataMoex.Mappers.Errors;

namespace TestHistoryData.Phase7_5;

public class SourceTimezonesTests
{
    [Fact]
    public void Resolve_EuropeMoscow_ReturnsTimeZone()
    {
        TimeZoneInfo tz = SourceTimezones.Resolve("Europe/Moscow");
        Assert.NotNull(tz);
    }

    [Fact]
    public void Resolve_EuropeMoscow_OffsetIs3Hours()
    {
        TimeZoneInfo tz = SourceTimezones.Resolve("Europe/Moscow");
        Assert.Equal(TimeSpan.FromHours(3), tz.BaseUtcOffset);
    }

    [Fact]
    public void Resolve_UnknownTimezone_ThrowsMappingTimezoneException()
    {
        var ex = Assert.Throws<MappingTimezoneException>(
            () => SourceTimezones.Resolve("Invalid/Tz"));
        Assert.Equal("Invalid/Tz", ex.IanaId);
        Assert.Equal("mapping_timezone", ex.ErrorCategory);
    }
}
