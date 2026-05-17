namespace History_DataMoex.Models;

public sealed record TradeStats5m
{
    public required string SecId { get; init; }
    public required DateTime BeginUtc { get; init; }
    public required DateTime BeginLocal { get; init; }
    public required int IntervalSeconds { get; init; }
    public required string Source { get; init; }

    public double? PrOpen { get; init; }
    public double? PrHigh { get; init; }
    public double? PrLow { get; init; }
    public double? PrClose { get; init; }
    public double? PrStd { get; init; }

    public long? Vol { get; init; }
    public double? Val { get; init; }
    public long? Trades { get; init; }

    public double? PrVwap { get; init; }
    public double? PrChange { get; init; }

    public long? TradesB { get; init; }
    public long? TradesS { get; init; }
    public double? ValB { get; init; }
    public double? ValS { get; init; }
    public long? VolB { get; init; }
    public long? VolS { get; init; }

    public double? Disb { get; init; }
    public double? PrVwapB { get; init; }
    public double? PrVwapS { get; init; }

    public int? SecPrOpen { get; init; }
    public int? SecPrHigh { get; init; }
    public int? SecPrLow { get; init; }
    public int? SecPrClose { get; init; }

    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
}
