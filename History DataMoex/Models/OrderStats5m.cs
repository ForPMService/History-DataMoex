namespace History_DataMoex.Models;

public sealed record OrderStats5m
{
    public required string SecId { get; init; }
    public required DateTime BeginUtc { get; init; }
    public required DateTime BeginLocal { get; init; }
    public required int IntervalSeconds { get; init; }
    public required string Source { get; init; }

    public long? PutOrdersB { get; init; }
    public long? PutOrdersS { get; init; }
    public double? PutValB { get; init; }
    public double? PutValS { get; init; }
    public long? PutVolB { get; init; }
    public long? PutVolS { get; init; }
    public double? PutVwapB { get; init; }
    public double? PutVwapS { get; init; }
    public long? PutVol { get; init; }
    public double? PutVal { get; init; }
    public long? PutOrders { get; init; }

    public long? CancelOrdersB { get; init; }
    public long? CancelOrdersS { get; init; }
    public double? CancelValB { get; init; }
    public double? CancelValS { get; init; }
    public long? CancelVolB { get; init; }
    public long? CancelVolS { get; init; }
    public double? CancelVwapB { get; init; }
    public double? CancelVwapS { get; init; }
    public long? CancelVol { get; init; }
    public double? CancelVal { get; init; }
    public long? CancelOrders { get; init; }

    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
}
