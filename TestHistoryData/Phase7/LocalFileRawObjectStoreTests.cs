using History_DataMoex.Mappers;
using History_DataMoex.Options;
using History_DataMoex.RawStore;
using System.Text.Json;

namespace TestHistoryData.Phase7;

public class LocalFileRawObjectStoreTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(
        Path.GetTempPath(),
        "rawstore_test_" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private LocalFileRawObjectStore CreateStore()
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            new RawStoreOptions { Root = _tempRoot });
        return new LocalFileRawObjectStore(options);
    }

    private static MapContext CreateStoreTestContext() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "/datashop/algopack/eq/candles/SBER.json",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    [Fact]
    public async Task Save_CreatesFileAndManifest()
    {
        var store = CreateStore();
        var context = CreateStoreTestContext();
        byte[] content = new byte[] { 1, 2, 3, 4, 5 };

        RawObjectMeta meta = await store.SaveAsync(
            content, context, "SBER", "2026-05-07", "2026-05-07");

        string fullRawPath = Path.Combine(_tempRoot,
            meta.StoragePath.Replace('/', Path.DirectorySeparatorChar));
        string fullManifestPath = Path.Combine(_tempRoot,
            meta.ManifestPath.Replace('/', Path.DirectorySeparatorChar));

        Assert.True(File.Exists(fullRawPath));
        Assert.True(File.Exists(fullManifestPath));
        Assert.False(Path.IsPathRooted(meta.StoragePath));
        Assert.False(Path.IsPathRooted(meta.ManifestPath));
        Assert.False(meta.StoragePath.Contains('\\'));
        Assert.False(meta.ManifestPath.Contains('\\'));

        using JsonDocument _ = JsonDocument.Parse(File.ReadAllText(fullManifestPath));
    }

    [Fact]
    public async Task Save_ManifestContainsCorrectFields()
    {
        var store = CreateStore();
        var context = CreateStoreTestContext();
        byte[] content = new byte[] { 10, 20, 30, 40 };
        const string secId = "SBER";
        const string fromDate = "2026-05-07";
        const string tillDate = "2026-05-07";

        RawObjectMeta meta = await store.SaveAsync(
            content, context, secId, fromDate, tillDate);

        string fullManifestPath = Path.Combine(_tempRoot,
            meta.ManifestPath.Replace('/', Path.DirectorySeparatorChar));

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(fullManifestPath));
        JsonElement root = doc.RootElement;

        Assert.Equal(context.RawObjectId.ToString("D"), root.GetProperty("raw_object_id").GetString());
        Assert.Equal(context.LoadJobId.ToString("D"), root.GetProperty("load_job_id").GetString());
        Assert.Equal(context.SourceCode, root.GetProperty("source_code").GetString());
        Assert.Equal(context.Endpoint, root.GetProperty("endpoint").GetString());
        Assert.Equal(secId, root.GetProperty("sec_id").GetString());
        Assert.Equal(fromDate, root.GetProperty("from").GetString());
        Assert.Equal(tillDate, root.GetProperty("till").GetString());
        Assert.Equal(meta.Sha256Hex, root.GetProperty("sha256").GetString());
        Assert.Equal(content.Length, root.GetProperty("bytes_length").GetInt64());

        string? fetchedAtStr = root.GetProperty("fetched_at_utc").GetString();
        DateTime fetchedAt = DateTime.Parse(fetchedAtStr!, null,
            System.Globalization.DateTimeStyles.RoundtripKind);
        Assert.Equal(context.FetchedAtUtc, fetchedAt.ToUniversalTime());
    }

    [Fact]
    public async Task Save_SameContent_Idempotent()
    {
        var store = CreateStore();
        byte[] content = new byte[] { 7, 8, 9 };

        RawObjectMeta meta1 = await store.SaveAsync(
            content, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07");

        RawObjectMeta meta2 = await store.SaveAsync(
            content, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07");

        Assert.False(meta1.AlreadyExisted);
        Assert.True(meta2.AlreadyExisted);
        Assert.Equal(meta1.Sha256Hex, meta2.Sha256Hex);
    }

    [Fact]
    public async Task Save_DifferentContent_DifferentFile()
    {
        var store = CreateStore();
        byte[] content1 = new byte[] { 1, 2, 3 };
        byte[] content2 = new byte[] { 4, 5, 6 };

        RawObjectMeta meta1 = await store.SaveAsync(
            content1, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07");

        RawObjectMeta meta2 = await store.SaveAsync(
            content2, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07");

        Assert.NotEqual(meta1.Sha256Hex, meta2.Sha256Hex);

        string fullRawPath1 = Path.Combine(_tempRoot,
            meta1.StoragePath.Replace('/', Path.DirectorySeparatorChar));
        string fullRawPath2 = Path.Combine(_tempRoot,
            meta2.StoragePath.Replace('/', Path.DirectorySeparatorChar));

        Assert.True(File.Exists(fullRawPath1));
        Assert.True(File.Exists(fullRawPath2));
    }

    [Fact]
    public async Task Save_DirectoryStructure_Correct()
    {
        var store = CreateStore();
        var context = new MapContext(
            SourceCode: "MOEX_ALGOPACK",
            Endpoint: "/datashop/algopack/eq/candles/SBER.json",
            SourceTimezone: "Europe/Moscow",
            LoadJobId: Guid.CreateVersion7(),
            RawObjectId: Guid.CreateVersion7(),
            FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

        RawObjectMeta meta = await store.SaveAsync(
            new byte[] { 99 }, context, "SBER", "2026-05-07", "2026-05-07");

        Assert.StartsWith("MOEX_ALGOPACK/", meta.StoragePath);
        Assert.Contains("datashop-algopack-eq-candles-SBER", meta.StoragePath);
        Assert.Contains("SBER/2026-05-07_2026-05-07/", meta.StoragePath);
        Assert.EndsWith(".json", meta.StoragePath);
        Assert.False(meta.StoragePath.EndsWith(".manifest.json"));
    }
}
