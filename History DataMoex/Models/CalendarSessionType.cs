namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Session Type (справочник типов сессий).
/// Источник: CalendarSessionTypeDTO. Group C + market (Lock §9).
/// Market — внешний параметр Map (одна DTO для stock и futures session_schedule.types).
/// Type — required бизнес-ключ.
/// </summary>
public sealed record CalendarSessionType
{
    public required string Market { get; init; }
    public required string Type { get; init; }
    public string? Title { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
