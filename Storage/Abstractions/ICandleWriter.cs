using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Пакетная запись минутных свечей в аналитическое хранилище.
/// В будущей реализации целевое хранилище — ClickHouse.
/// </summary>
public interface ICandleWriter
{
    Task WriteAsync(
        IReadOnlyList<Candle1m> rows,
        Guid rawObjectId,
        Guid jobId,
        CancellationToken ct);
}