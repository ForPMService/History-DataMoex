using History_DataMoex.Contracts.Ingestion;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Хранилище исходных ответов источника.
/// В фазе реализации будет сохранять тело raw JSON в object storage,
/// а метаданные raw object — в PostgreSQL.
/// </summary>
public interface IRawObjectStore
{
    Task<Guid> SaveAsync(RawObject obj, Stream body, CancellationToken ct);

    Task<RawObject?> GetMetadataAsync(Guid id, CancellationToken ct);

    Task<Stream?> OpenBodyAsync(Guid id, CancellationToken ct);
}