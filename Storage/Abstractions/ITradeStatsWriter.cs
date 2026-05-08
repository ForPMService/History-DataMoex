using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Пакетная запись статистики сделок в аналитическое хранилище.
/// В будущей реализации целевое хранилище — ClickHouse.
/// </summary>
public interface ITradeStatsWriter
{
    Task WriteAsync(
        IReadOnlyList<TradeStats5m> rows,
        Guid rawObjectId,
        Guid jobId,
        CancellationToken ct);
}