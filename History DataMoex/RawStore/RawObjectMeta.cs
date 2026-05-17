namespace History_DataMoex.RawStore;

/// <summary>
/// Результат сохранения raw object.
/// StoragePath и ManifestPath — portable relative paths с разделителем '/'.
/// Для доступа к физическому файлу: Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar))
/// </summary>
public sealed record RawObjectMeta(
    Guid RawObjectId,
    string StoragePath,       // portable relative path к JSON-файлу (разделитель '/')
    string ManifestPath,      // portable relative path к manifest (разделитель '/')
    string Sha256Hex,         // hex-строка SHA-256 (lowercase)
    long BytesLength,
    bool AlreadyExisted);     // true если файл с таким SHA уже был на диске
