namespace History_DataMoex.RawStore;

/// <summary>
/// Тонкая абстракция над операциями ФС для LocalFileRawObjectStore.
/// internal — не часть публичного API. Существует только для testability:
/// без seam fake-race сценарии на File.Move детерминированно не воспроизводимы.
/// </summary>
internal interface IRawStoreFileSystem
{
    bool FileExists(string path);
    void CreateDirectory(string path);
    Task WriteAllBytesAsync(string path, ReadOnlyMemory<byte> content, CancellationToken ct);
    void Move(string source, string destination);
    void DeleteIfExists(string path);
}
