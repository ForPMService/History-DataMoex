using History_DataMoex.Mappers;
using History_DataMoex.Options;
using History_DataMoex.RawStore;
using History_DataMoex.RawStore.Errors;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TestHistoryData.TestUtilities;

namespace TestHistoryData.Phase7_5;

public class RawStoreErrorsTests
{
    private static MapContext CreateCtx() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "/datashop/algopack/eq/candles/SBER.json",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public void RawStoreConfigException_HasErrorCategory_NotRetryable()
    {
        var ex = new RawStoreConfigException("missing root");
        Assert.Equal("raw_config", ex.ErrorCategory);
        Assert.False(ex.IsRetryable);
    }

    [Fact]
    public void RawStorePathException_HasErrorCategory_NotRetryable()
    {
        var ex = new RawStorePathException("../etc");
        Assert.Equal("raw_path", ex.ErrorCategory);
        Assert.False(ex.IsRetryable);
        Assert.Equal("../etc", ex.Segment);
    }

    [Fact]
    public void RawStoreIOException_HasErrorCategory_IsRetryable()
    {
        var ex = new RawStoreIOException("/tmp/foo", new IOException("disk full"));
        Assert.Equal("raw_io", ex.ErrorCategory);
        Assert.True(ex.IsRetryable);
        Assert.Equal("/tmp/foo", ex.StoragePath);
    }

    [Fact]
    public void Constructor_EmptyRoot_ThrowsRawStoreConfigException()
    {
        var options = Options.Create(new RawStoreOptions { Root = "" });
        Assert.Throws<RawStoreConfigException>(() =>
            new LocalFileRawObjectStore(options, NullLogger<LocalFileRawObjectStore>.Instance));
    }

    [Fact]
    public async Task SanitizePathSegment_TraversalAttempt_ThrowsRawStorePathException()
    {
        var fakeFs = new FakeRawStoreFileSystem();
        var options = Options.Create(new RawStoreOptions { Root = Path.GetTempPath() });
        var store = new LocalFileRawObjectStore(
            options, NullLogger<LocalFileRawObjectStore>.Instance, fakeFs);

        ReadOnlyMemory<byte> payload = new byte[] { 1, 2, 3 };

        await Assert.ThrowsAsync<RawStorePathException>(() =>
            store.SaveAsync(payload, CreateCtx(), "../etc", "2026-05-07", "2026-05-07"));
    }
}
