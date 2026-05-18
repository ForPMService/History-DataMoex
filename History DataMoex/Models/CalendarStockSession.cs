namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Stock Session Schedule (расписание сессии stock рынка).
/// Источник: CalendarStockSessionDTO. Group B — SecId nullable (Lock §9, board/session-level).
/// TimeFrom/TimeTill — TimeOnly? (Lock §4 — time-only НЕ конвертируется в UTC).
/// UpdateTimeUtc — MSK→UTC через SourceDateTimeParser.
/// </summary>
public sealed record CalendarStockSession
{
    public required DateOnly TradeDate { get; init; }
    public int? TradingSession { get; init; }
    public string? BoardId { get; init; }
    public string? SecId { get; init; }
    public string? Type { get; init; }
    public TimeOnly? TimeFrom { get; init; }
    public TimeOnly? TimeTill { get; init; }
    public DateTime? UpdateTimeUtc { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
