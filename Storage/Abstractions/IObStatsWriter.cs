using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Пакетная запись статистики стакана в аналитическое хранилище.
/// В будущей реализации целевое хранилище — ClickHouse.
/// </summary>
public interface IObStatsWriter
{
    Task WriteAsync(
        IReadOnlyList<ObStats5m> rows,
        Guid rawObjectId,
        Guid jobId,
        CancellationToken ct);
}