namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Suspension Reason (справочник причин приостановки).
/// Источник: CalendarSuspendedReasonDTO. Group C — SecId not applicable (Lock §9).
/// Id — required PK (на него ссылается CalendarSuspension.ReasonId).
/// </summary>
public sealed record CalendarSuspensionReason
{
    public required int Id { get; init; }
    public string? Title { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
