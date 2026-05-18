namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Futures Session Schedule (расписание сессии futures рынка).
/// Источник: CalendarFuturesSessionDTO. Group B — SecId nullable (Lock §9).
/// TimeFromUtc/TimeTillUtc — full timestamp (НЕ time-only), MSK→UTC (Lock §4).
/// </summary>
public sealed record CalendarFuturesSession
{
    public required DateOnly TradeSessionDate { get; init; }
    public string? BoardId { get; init; }
    public string? SecId { get; init; }
    public string? Type { get; init; }
    public DateTime? TimeFromUtc { get; init; }
    public DateTime? TimeTillUtc { get; init; }
    public DateTime? UpdateTimeUtc { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
