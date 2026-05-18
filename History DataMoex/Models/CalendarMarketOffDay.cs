namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Market OffDay (выходной конкретного рынка stock/futures).
/// Источник: CalendarOffDaysMarketDTO. Group C + market (Lock §9).
/// Market — внешний параметр Map (одна DTO для двух рынков).
/// UpdateTimeUtc — MSK→UTC (Lock §4).
/// </summary>
public sealed record CalendarMarketOffDay
{
    public required string Market { get; init; }
    public required DateOnly TradeDate { get; init; }
    public int? IsTraded { get; init; }
    public DateOnly? TradeSessionDate { get; init; }
    public string? Reason { get; init; }
    public DateTime? UpdateTimeUtc { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
