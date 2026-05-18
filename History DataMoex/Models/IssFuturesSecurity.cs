namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель ISS Futures Security (snapshot справочника фьючерсов по конкретной board).
/// Источник: FuturesSecurityDTO (ISS engines/futures/markets/forts/boards/{board}/securities).
/// SecIdRequired (Lock §9). Snapshot policy (Lock §6). Цены — decimal? (Lock §7).
/// </summary>
public sealed record IssFuturesSecurity
{
    public required string SecId { get; init; }
    public string? ShortName { get; init; }
    public string? SecName { get; init; }
    public string? AssetCode { get; init; }
    public decimal? InitialMargin { get; init; }
    public decimal? PrevSettlePrice { get; init; }
    public decimal? MinStep { get; init; }
    public decimal? HighLimit { get; init; }
    public decimal? LowLimit { get; init; }
    public decimal? StepPrice { get; init; }
    public decimal? PrevPrice { get; init; }
    public int? Decimals { get; init; }
    public int? LotVolume { get; init; }
    public long? PrevOpenPosition { get; init; }
    public DateOnly? LastTradeDate { get; init; }
    public DateOnly? LastDelDate { get; init; }

    public required string Source { get; init; }
    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
