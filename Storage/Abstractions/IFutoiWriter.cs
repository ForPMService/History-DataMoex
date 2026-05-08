using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Пакетная запись открытого интереса в аналитическое хранилище.
/// В будущей реализации целевое хранилище — ClickHouse.
/// </summary>
public interface IFutoiWriter
{
    Task WriteAsync(
        IReadOnlyList<Futoi> rows,
        Guid rawObjectId,
        Guid jobId,
        CancellationToken ct);
}