namespace History_DataMoex.Normalization.Models
{
    /// <summary>
    /// Каноническая запись торгового календаря.
    /// </summary>
    public sealed record TradingCalendarEntry(
        string BoardId,
        DateOnly TradeDate,
        bool IsTradingDay,
        string? SessionType,
        TimeOnly? TimeFromUtc,
        TimeOnly? TimeTillUtc);
}