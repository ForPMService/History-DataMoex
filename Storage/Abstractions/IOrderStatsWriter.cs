using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Пакетная запись статистики заявок в аналитическое хранилище.
/// В будущей реализации целевое хранилище — ClickHouse.
/// </summary>
public interface IOrderStatsWriter
{
    Task WriteAsync(
        IReadOnlyList<OrderStats5m> rows,
        Guid rawObjectId,
        Guid jobId,
        CancellationToken ct);
}