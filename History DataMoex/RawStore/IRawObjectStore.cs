using History_DataMoex.Mappers;

namespace History_DataMoex.RawStore;

/// <summary>
/// Контракт сохранения сырого ответа источника.
/// Реализация: LocalFileRawObjectStore (файловая система).
/// </summary>
public interface IRawObjectStore
{
    Task<RawObjectMeta> SaveAsync(
        ReadOnlyMemory<byte> content,
        MapContext context,
        string secId,
        string fromDate,
        string tillDate,
        CancellationToken ct = default);
}
