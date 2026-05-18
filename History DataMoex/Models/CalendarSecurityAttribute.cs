namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Security Attribute (справочник атрибутов бумаги).
/// Источник: CalendarSecurityAttributeDTO. Group C — SecId not applicable (Lock §9).
/// Name — required бизнес-ключ (на это имя ссылается CalendarSecurityChange.AttributeName).
/// </summary>
public sealed record CalendarSecurityAttribute
{
    public required string Name { get; init; }
    public string? Type { get; init; }
    public string? Title { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
