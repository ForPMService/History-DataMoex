namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель Forts Contract (фьючерсный контракт справочника Calendar).
/// Источник: CalendarFortsContractDTO. Group A — SecIdRequired (Lock §9).
/// </summary>
public sealed record CalendarFortsContract
{
    public required string SecId { get; init; }
    public string? AssetCode { get; init; }
    public string? ShortName { get; init; }
    public string? ExecType { get; init; }
    public string? ContractName { get; init; }
    public DateOnly? ExpirationDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? ExpirationType { get; init; }
    public TimeOnly? ExpirationTime { get; init; }
    public int? WeekendSession { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
