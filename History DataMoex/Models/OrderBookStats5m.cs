namespace History_DataMoex.Models;

public sealed record OrderBookStats5m
{
    public required string SecId { get; init; }
    public required DateTime BeginUtc { get; init; }
    public required DateTime BeginLocal { get; init; }
    public required int IntervalSeconds { get; init; }
    public required string Source { get; init; }

    public double? SpreadBbo { get; init; }
    public double? SpreadLv10 { get; init; }
    public double? Spread1Mio { get; init; }

    public int? LevelsB { get; init; }
    public int? LevelsS { get; init; }

    public long? VolB { get; init; }
    public long? VolS { get; init; }
    public long? ValB { get; init; }
    public long? ValS { get; init; }

    public double? ImbalanceVolBbo { get; init; }
    public double? ImbalanceValBbo { get; init; }
    public double? ImbalanceVol { get; init; }
    public double? ImbalanceVal { get; init; }

    public double? VwapB { get; init; }
    public double? VwapS { get; init; }
    public double? VwapB1Mio { get; init; }
    public double? VwapS1Mio { get; init; }

    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
}
