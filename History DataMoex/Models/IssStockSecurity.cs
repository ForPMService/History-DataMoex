namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель ISS Stock Security (snapshot справочника акций по конкретной board).
/// Источник: StockSecurityDTO (ISS engines/stock/markets/shares/boards/{board}/securities).
/// SecIdRequired (Lock §9). Snapshot policy (Lock §6) — FetchedAtUtc обязателен.
/// </summary>
public sealed record IssStockSecurity
{
    public required string SecId { get; init; }
    public required string BoardId { get; init; }
    public string? ShortName { get; init; }
    public string? SecName { get; init; }
    public string? MarketCode { get; init; }
    public decimal? PrevLegalClosePrice { get; init; }
    public int? LotSize { get; init; }
    public decimal? FaceValue { get; init; }
    public DateOnly? PrevDate { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
