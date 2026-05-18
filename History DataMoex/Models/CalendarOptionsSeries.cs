namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Options Series (серия опционов).
/// Источник: CalendarOptionsSeriesDTO. Group C — SecId not applicable (Lock §9).
/// AssetCode — бизнес-идентификатор серии (опционы привязаны к asset, не к конкретной бумаге).
/// </summary>
public sealed record CalendarOptionsSeries
{
    public string? AssetTypeName { get; init; }
    public string? AssetCode { get; init; }
    public string? SeriesName { get; init; }
    public string? SeriesType { get; init; }
    public string? ExecType { get; init; }
    public string? MarginStyle { get; init; }
    public string? ContractName { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public string? ExpirationType { get; init; }
    public TimeOnly? ExpirationTime { get; init; }
    public int? WeekendSession { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
