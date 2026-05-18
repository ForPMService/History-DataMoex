namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель OffDay (общий выходной по всем 3 рынкам).
/// Источник: CalendarOffDaysAllDTO. Group C — SecId not applicable (Lock §9).
/// Без market параметра (все 3 рынка в одной строке).
/// </summary>
public sealed record CalendarOffDayAll
{
    public required DateOnly TradeDate { get; init; }
    public int? CurrencyWorkday { get; init; }
    public DateOnly? CurrencyTradeSessionDate { get; init; }
    public string? CurrencyReason { get; init; }
    public int? FuturesWorkday { get; init; }
    public DateOnly? FuturesTradeSessionDate { get; init; }
    public string? FuturesReason { get; init; }
    public int? StockWorkday { get; init; }
    public DateOnly? StockTradeSessionDate { get; init; }
    public string? StockReason { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
