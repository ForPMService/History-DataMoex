using History_DataMoex.RawStore;

namespace TestHistoryData.TestUtilities;

internal sealed class FakeRawStoreFileSystem : IRawStoreFileSystem
{
    public Func<string, bool> FileExistsImpl { get; set; } = _ => false;
    public Action<string> CreateDirectoryImpl { get; set; } = _ => { };
    public Func<string, ReadOnlyMemory<byte>, CancellationToken, Task> WriteAllBytesAsyncImpl { get; set; }
        = (_, _, _) => Task.CompletedTask;
    public Action<string, string> MoveImpl { get; set; } = (_, _) => { };
    public Action<string> DeleteIfExistsImpl { get; set; } = _ => { };

    public bool FileExists(string path) => FileExistsImpl(path);
    public void CreateDirectory(string path) => CreateDirectoryImpl(path);
    public Task WriteAllBytesAsync(string p, ReadOnlyMemory<byte> c, CancellationToken ct) => WriteAllBytesAsyncImpl(p, c, ct);
    public void Move(string s, string d) => MoveImpl(s, d);
    public void DeleteIfExists(string p) => DeleteIfExistsImpl(p);
}
