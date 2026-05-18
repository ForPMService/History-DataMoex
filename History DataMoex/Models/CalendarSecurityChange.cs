namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Security Change (изменение атрибута бумаги).
/// Источник: CalendarSecurityChangeDTO. Group A — SecIdRequired (Lock §9).
/// UpdateTimeUtc — required бизнес-ключ (момент изменения), MSK→UTC (Lock §4).
/// </summary>
public sealed record CalendarSecurityChange
{
    public required DateTime UpdateTimeUtc { get; init; }
    public string? Action { get; init; }
    public required string SecId { get; init; }
    public string? AttributeName { get; init; }
    public string? BeforeValue { get; init; }
    public string? AfterValue { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
