namespace History_DataMoex.Models;

public sealed record MegaAlertsStock
{
    public required string SecId { get; init; }
    public required DateTime BeginUtc { get; init; }
    public required DateTime BeginLocal { get; init; }
    public required string Source { get; init; }

    public string? AlertType { get; init; }
    public double? Threshold { get; init; }
    public double? Value { get; init; }
    public string? Reference { get; init; }

    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
}
