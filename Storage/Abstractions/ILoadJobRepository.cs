using History_DataMoex.Contracts.Ingestion;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Репозиторий задач.
/// PostgreSQL остаётся источником истины по статусам jobs.
/// </summary>
public interface ILoadJobRepository
{
    Task<LoadJob?> GetByIdAsync(Guid jobId, CancellationToken ct);

    Task<IReadOnlyList<LoadJob>> GetRunnableAsync(int limit, CancellationToken ct);

    Task UpdateStatusAsync(
        Guid jobId,
        JobStatus status,
        string? lastError,
        DateTime? finishedAtUtc,
        CancellationToken ct);
}