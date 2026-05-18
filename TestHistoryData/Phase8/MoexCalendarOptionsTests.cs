using History_DataMoex.Options;

namespace TestHistoryData.Phase8;

public class MoexCalendarOptionsTests
{
    [Fact]
    public void MoexCalendarOptions_InheritsFromMoexClientOptions()
    {
        var options = new MoexCalendarOptions
        {
            BaseUrl = "https://example.com/iss",
            UserAgent = "test/1.0",
        };

        Assert.IsAssignableFrom<MoexClientOptions>(options);
        Assert.Equal("https://example.com/iss", options.BaseUrl);
        Assert.Equal("test/1.0", options.UserAgent);
    }

    [Fact]
    public void MoexCalendarOptions_Key_CanBeSetAndRead()
    {
        var options = new MoexCalendarOptions { Key = "abc" };
        Assert.Equal("abc", options.Key);

        // Default — пустая строка, не null.
        Assert.Equal(string.Empty, new MoexCalendarOptions().Key);
    }
}
