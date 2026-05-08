using History_DataMoex.Contracts.Ingestion;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Репозиторий справочника инструментов.
/// В будущей реализации будет работать с PostgreSQL.
/// </summary>
public interface IInstrumentRepository
{
    Task<Instrument?> GetByIdAsync(Guid instrumentId, CancellationToken ct);

    Task<Instrument?> FindBySourceCodeAsync(
        string secId,
        string? boardId,
        string? assetCode,
        CancellationToken ct);

    Task<IReadOnlyList<Instrument>> ListActiveAsync(CancellationToken ct);
}