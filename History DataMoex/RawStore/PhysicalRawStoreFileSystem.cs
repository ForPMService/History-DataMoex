namespace History_DataMoex.RawStore;

internal sealed class PhysicalRawStoreFileSystem : IRawStoreFileSystem
{
    public static PhysicalRawStoreFileSystem Instance { get; } = new();

    private PhysicalRawStoreFileSystem() { }

    public bool FileExists(string path) => File.Exists(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    /// <summary>
    /// Запись через FileStream с FileMode.CreateNew — явный запрет overwrite.
    /// Defense-in-depth для уникального temp suffix в SaveAsync: если suffix
    /// каким-то образом совпадёт, получим IOException сразу, а не молчаливую перезапись.
    /// content передаётся как ReadOnlyMemory&lt;byte&gt; без копий (без .ToArray()).
    /// </summary>
    public async Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> content, CancellationToken ct)
    {
        await using FileStream stream = new(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            useAsync: true);

        await stream.WriteAsync(content, ct).ConfigureAwait(false);
    }

    public void Move(string source, string destination) => File.Move(source, destination);

    public void DeleteIfExists(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // best-effort cleanup
        }
    }
}
