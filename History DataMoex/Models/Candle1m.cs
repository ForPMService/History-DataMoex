namespace History_DataMoex.Models;

/// <summary>
/// Внутренняя модель 1-минутной свечи.
/// Не DTO от MOEX — модель витрины, готовая к записи в хранилище.
/// </summary>
public sealed record Candle1m
{
    public required string SecId { get; init; }
    public required DateTime BeginUtc { get; init; }       // DateTimeKind.Utc
    public required DateTime BeginLocal { get; init; }     // оригинальное время как пришло от источника
    public required int IntervalSeconds { get; init; }     // 60
    public required string Source { get; init; }            // "MOEX_ALGOPACK"
    public double? Open { get; init; }
    public double? High { get; init; }
    public double? Low { get; init; }
    public double? Close { get; init; }
    public double? Volume { get; init; }
    public double? Value { get; init; }
    public required ulong RowHashV1 { get; init; }         // xxHash64
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
}
