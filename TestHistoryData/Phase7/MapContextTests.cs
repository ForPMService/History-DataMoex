using History_DataMoex.Mappers;

namespace TestHistoryData.Phase7;

public class MapContextTests
{
    [Fact]
    public void CreateVersion7_NotEmpty()
    {
        var guid = Guid.CreateVersion7();

        Assert.NotEqual(Guid.Empty, guid);
    }

    [Fact]
    public void CreateVersion7_TwoCallsReturnDifferentValues()
    {
        var g1 = Guid.CreateVersion7();
        var g2 = Guid.CreateVersion7();

        Assert.NotEqual(g1, g2);
    }
}
