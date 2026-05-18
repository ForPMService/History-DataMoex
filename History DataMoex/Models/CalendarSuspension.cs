namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Suspension (приостановка торгов конкретной бумагой).
/// Источник: CalendarSuspendedDTO. Group A — SecIdRequired (Lock §9).
/// UpdateTime → UpdateTimeUtc через MSK→UTC (Lock §4).
/// ReasonId нормализован из string? (DTO) в int? (модель) — FK к CalendarSuspensionReason.Id.
/// </summary>
public sealed record CalendarSuspension
{
    public required string SecId { get; init; }
    public int? ReasonId { get; init; }
    public DateOnly? DateFrom { get; init; }
    public DateOnly? DateTill { get; init; }
    public string? BoardId { get; init; }
    public string? SettleCodes { get; init; }
    public DateOnly? ChangeDate { get; init; }
    public DateTime? UpdateTimeUtc { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
