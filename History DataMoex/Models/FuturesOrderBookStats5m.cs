namespace History_DataMoex.Models;

public sealed record FuturesOrderBookStats5m
{
    public required string SecId { get; init; }
    public string? AssetCode { get; init; }
    public required DateTime BeginUtc { get; init; }
    public required DateTime BeginLocal { get; init; }
    public required int IntervalSeconds { get; init; }
    public required string Source { get; init; }

    public double? MidPrice { get; init; }
    public double? MicroPrice { get; init; }

    public double? SpreadL1 { get; init; }
    public double? SpreadL2 { get; init; }
    public double? SpreadL3 { get; init; }
    public double? SpreadL5 { get; init; }
    public double? SpreadL10 { get; init; }
    public double? SpreadL20 { get; init; }

    public int? LevelsB { get; init; }
    public int? LevelsS { get; init; }

    public long? VolBL1 { get; init; }
    public long? VolBL2 { get; init; }
    public long? VolBL3 { get; init; }
    public long? VolBL5 { get; init; }
    public long? VolBL10 { get; init; }
    public long? VolBL20 { get; init; }

    public long? VolSL1 { get; init; }
    public long? VolSL2 { get; init; }
    public long? VolSL3 { get; init; }
    public long? VolSL5 { get; init; }
    public long? VolSL10 { get; init; }
    public long? VolSL20 { get; init; }

    public double? VwapBL3 { get; init; }
    public double? VwapBL5 { get; init; }
    public double? VwapBL10 { get; init; }
    public double? VwapBL20 { get; init; }

    public double? VwapSL3 { get; init; }
    public double? VwapSL5 { get; init; }
    public double? VwapSL10 { get; init; }
    public double? VwapSL20 { get; init; }

    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
}
