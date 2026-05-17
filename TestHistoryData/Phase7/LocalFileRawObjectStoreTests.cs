using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using History_DataMoex.Mappers;
using History_DataMoex.Options;
using History_DataMoex.RawStore;
using History_DataMoex.RawStore.Errors;
using Microsoft.Extensions.Logging.Abstractions;
using TestHistoryData.TestUtilities;

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
        return new LocalFileRawObjectStore(
            options, NullLogger<LocalFileRawObjectStore>.Instance);
    }

    private LocalFileRawObjectStore CreateStoreWithFs(IRawStoreFileSystem fs)
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            new RawStoreOptions { Root = _tempRoot });
        return new LocalFileRawObjectStore(
            options, NullLogger<LocalFileRawObjectStore>.Instance, fs);
    }

    private LocalFileRawObjectStore CreateStoreWithFs(IRawStoreFileSystem fs, ListLogger<LocalFileRawObjectStore> logger)
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            new RawStoreOptions { Root = _tempRoot });
        return new LocalFileRawObjectStore(options, logger, fs);
    }

    private static MapContext CreateStoreTestContext() => new(
        SourceCode: "MOEX_ALGOPACK",
        Endpoint: "/datashop/algopack/eq/candles/SBER.json",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 17, 14, 30, 0, DateTimeKind.Utc));

    // ── Existing phase 7 tests (kept green after refactor) ──────────────────

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

    // ── Phase 7.5 §9.7 additions ────────────────────────────────────────────

    [Fact]
    public async Task Save_WithLogger_DoesNotThrow()
    {
        var store = CreateStore();
        byte[] content = new byte[] { 1, 2, 3 };

        RawObjectMeta meta = await store.SaveAsync(
            content, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07");

        Assert.False(meta.AlreadyExisted);
    }

    [Fact]
    public async Task Save_ConcurrentSameContent_BothReturnSuccess()
    {
        var store = CreateStore();
        byte[] content = new byte[] { 50, 51, 52, 53 };

        Task<RawObjectMeta>[] tasks = new[]
        {
            store.SaveAsync(content, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07"),
            store.SaveAsync(content, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07"),
        };

        RawObjectMeta[] results = await Task.WhenAll(tasks);

        Assert.Equal(results[0].Sha256Hex, results[1].Sha256Hex);
        Assert.True(results[0].AlreadyExisted ^ results[1].AlreadyExisted,
            "Exactly one of the two concurrent saves should report AlreadyExisted=true");
    }

    [Fact]
    public async Task Save_IOExceptionOnWrite_ThrowsRawStoreIOException()
    {
        var logger = new ListLogger<LocalFileRawObjectStore>();
        var fakeFs = new FakeRawStoreFileSystem
        {
            FileExistsImpl = _ => false,
            WriteAllBytesAsyncImpl = (_, _, _) => throw new IOException("disk full"),
        };
        var store = CreateStoreWithFs(fakeFs, logger);

        ReadOnlyMemory<byte> payload = new byte[] { 1, 2, 3 };

        var ex = await Assert.ThrowsAsync<RawStoreIOException>(() =>
            store.SaveAsync(payload, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07"));

        Assert.Equal("raw_io", ex.ErrorCategory);
        Assert.True(ex.IsRetryable);
        Assert.Contains(logger.Entries, e => e.EventId.Id == 203);
    }

    [Fact]
    public async Task Save_FileMoveRace_RealRaceCondition_ReturnsAlreadyExisted()
    {
        var logger = new ListLogger<LocalFileRawObjectStore>();
        bool rawTargetExists = false;
        bool manifestTargetExists = false;

        var fakeFs = new FakeRawStoreFileSystem();
        fakeFs.FileExistsImpl = path =>
        {
            if (path.EndsWith(".manifest.json")) return manifestTargetExists;
            if (path.EndsWith(".json")) return rawTargetExists;
            return false;
        };
        fakeFs.MoveImpl = (src, dst) =>
        {
            if (dst.EndsWith(".manifest.json"))
            {
                manifestTargetExists = true;
                return;
            }
            // First move: race — pretend target appeared.
            rawTargetExists = true;
            throw new IOException("simulated race");
        };

        var store = CreateStoreWithFs(fakeFs, logger);
        ReadOnlyMemory<byte> payload = new byte[] { 7, 8, 9 };

        RawObjectMeta meta = await store.SaveAsync(
            payload, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07");

        Assert.True(meta.AlreadyExisted);
        Assert.Contains(logger.Entries, e => e.EventId.Id == 202);
    }

    [Fact]
    public async Task Save_FileMoveRace_FakeRaceCondition_ThrowsRawStoreIOException()
    {
        var fakeFs = new FakeRawStoreFileSystem
        {
            FileExistsImpl = _ => false,
            WriteAllBytesAsyncImpl = (_, _, _) => Task.CompletedTask,
            MoveImpl = (_, _) => throw new IOException("simulated"),
        };

        var options = Microsoft.Extensions.Options.Options.Create(
            new RawStoreOptions { Root = _tempRoot });
        var store = new LocalFileRawObjectStore(
            options, NullLogger<LocalFileRawObjectStore>.Instance, fakeFs);

        ReadOnlyMemory<byte> payload = new byte[] { 1, 2, 3 };

        var ex = await Assert.ThrowsAsync<RawStoreIOException>(() =>
            store.SaveAsync(payload, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07"));

        Assert.Equal("raw_io", ex.ErrorCategory);
    }

    [Fact]
    public async Task Save_PathTraversalAttempt_ThrowsRawStorePathException()
    {
        var store = CreateStore();
        ReadOnlyMemory<byte> payload = new byte[] { 1, 2, 3 };

        await Assert.ThrowsAsync<RawStorePathException>(() =>
            store.SaveAsync(payload, CreateStoreTestContext(), "../etc", "2026-05-07", "2026-05-07"));
    }

    [Fact]
    public async Task Save_PassesContentToFileSystem_NotCopied()
    {
        int? observedRawLength = null;
        byte[]? observedRawSnapshot = null;

        var fakeFs = new FakeRawStoreFileSystem
        {
            FileExistsImpl = _ => false,
        };
        fakeFs.WriteAllBytesAsyncImpl = (path, content, _) =>
        {
            if (path.EndsWith(".manifest.tmp")) return Task.CompletedTask;
            observedRawLength = content.Length;
            observedRawSnapshot = content.ToArray();
            return Task.CompletedTask;
        };

        var store = CreateStoreWithFs(fakeFs);
        byte[] payload = new byte[] { 41, 42, 43, 44, 45 };

        await store.SaveAsync(payload, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07");

        Assert.Equal(payload.Length, observedRawLength);
        Assert.NotNull(observedRawSnapshot);
        Assert.Equal(payload, observedRawSnapshot);
    }

    [Fact]
    public async Task Save_UniqueTempPaths_ConcurrentWritesDoNotCollide()
    {
        var seenPaths = new ConcurrentBag<string>();
        var fakeFs = new FakeRawStoreFileSystem
        {
            FileExistsImpl = _ => false,
        };
        fakeFs.WriteAllBytesAsyncImpl = (path, _, _) =>
        {
            if (!path.EndsWith(".manifest.tmp") && path.EndsWith(".tmp"))
                seenPaths.Add(path);
            return Task.CompletedTask;
        };

        var store = CreateStoreWithFs(fakeFs);
        byte[] payload = new byte[] { 1, 2, 3, 4 };

        Task[] tasks = Enumerable.Range(0, 10).Select(_ =>
            (Task)store.SaveAsync(
                payload, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07")).ToArray();

        await Task.WhenAll(tasks);

        Assert.Equal(10, seenPaths.Count);
        Assert.Equal(seenPaths.Count, seenPaths.Distinct().Count());
    }

    [Fact]
    public async Task Save_ExistingRawMissingManifest_RecoversManifest()
    {
        var store = CreateStore();
        var ctx = CreateStoreTestContext();
        byte[] content = new byte[] { 11, 22, 33 };

        string sha = Convert.ToHexStringLower(SHA256.HashData(content));
        string fullDir = Path.Combine(
            _tempRoot,
            "MOEX_ALGOPACK",
            "datashop-algopack-eq-candles-SBER",
            "SBER",
            "2026-05-07_2026-05-07");
        Directory.CreateDirectory(fullDir);
        string rawFile = Path.Combine(fullDir, sha + ".json");
        string manifestFile = Path.Combine(fullDir, sha + ".manifest.json");
        await File.WriteAllBytesAsync(rawFile, content);
        Assert.False(File.Exists(manifestFile));

        RawObjectMeta meta = await store.SaveAsync(content, ctx, "SBER", "2026-05-07", "2026-05-07");

        Assert.True(meta.AlreadyExisted);
        Assert.True(File.Exists(manifestFile));

        using JsonDocument doc = JsonDocument.Parse(File.ReadAllText(manifestFile));
        JsonElement root = doc.RootElement;
        Assert.Equal(ctx.LoadJobId.ToString("D"), root.GetProperty("load_job_id").GetString());
        Assert.Equal(ctx.RawObjectId.ToString("D"), root.GetProperty("raw_object_id").GetString());
        Assert.Equal(sha, root.GetProperty("sha256").GetString());
    }

    [Fact]
    public async Task Save_RaceOnMove_AlsoWritesManifest()
    {
        bool rawTargetExists = false;
        bool manifestTargetExists = false;
        var writePaths = new List<string>();
        var movePaths = new List<(string Src, string Dst)>();

        var fakeFs = new FakeRawStoreFileSystem();
        fakeFs.FileExistsImpl = path =>
        {
            if (path.EndsWith(".manifest.json")) return manifestTargetExists;
            if (path.EndsWith(".json")) return rawTargetExists;
            return false;
        };
        fakeFs.WriteAllBytesAsyncImpl = (path, _, _) =>
        {
            lock (writePaths) writePaths.Add(path);
            return Task.CompletedTask;
        };
        fakeFs.MoveImpl = (src, dst) =>
        {
            lock (movePaths) movePaths.Add((src, dst));
            if (dst.EndsWith(".manifest.json"))
            {
                manifestTargetExists = true;
                return;
            }
            rawTargetExists = true;
            throw new IOException("simulated race");
        };

        var store = CreateStoreWithFs(fakeFs);
        ReadOnlyMemory<byte> payload = new byte[] { 1, 2 };

        RawObjectMeta meta = await store.SaveAsync(
            payload, CreateStoreTestContext(), "SBER", "2026-05-07", "2026-05-07");

        Assert.True(meta.AlreadyExisted);
        Assert.Contains(writePaths, p => p.EndsWith(".manifest.tmp"));
        Assert.Contains(movePaths, m => m.Dst.EndsWith(".manifest.json"));
    }
}
