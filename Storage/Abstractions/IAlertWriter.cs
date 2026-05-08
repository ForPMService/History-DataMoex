using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Пакетная запись рыночных алертов в аналитическое хранилище.
/// В будущей реализации целевое хранилище — ClickHouse.
/// </summary>
public interface IAlertWriter
{
    Task WriteAsync(
        IReadOnlyList<Alert> rows,
        Guid rawObjectId,
        Guid jobId,
        CancellationToken ct);
}