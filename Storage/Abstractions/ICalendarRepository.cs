using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Storage.Abstractions;

/// <summary>
/// Репозиторий торгового календаря.
/// В будущей реализации будет хранить торговые дни и сессии в PostgreSQL.
/// </summary>
public interface ICalendarRepository
{
    Task<IReadOnlyList<TradingCalendarEntry>> GetRangeAsync(
        string boardId,
        DateOnly from,
        DateOnly till,
        CancellationToken ct);

    Task<bool> IsTradingDayAsync(
        string boardId,
        DateOnly tradeDate,
        CancellationToken ct);
}