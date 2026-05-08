using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Пакетная запись HI2-данных в аналитическое хранилище.
/// В будущей реализации целевое хранилище — ClickHouse.
/// </summary>
public interface IHi2Writer
{
    Task WriteAsync(
        IReadOnlyList<Hi2> rows,
        Guid rawObjectId,
        Guid jobId,
        CancellationToken ct);
}