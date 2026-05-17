namespace History_DataMoex.Models;

public sealed record Hi2Stock
{
    public required string SecId { get; init; }
    public required DateTime BeginUtc { get; init; }
    public required DateTime BeginLocal { get; init; }
    public required string Source { get; init; }

    public string? Metric { get; init; }
    public double? Value { get; init; }
    public string? Reference { get; init; }

    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
}
