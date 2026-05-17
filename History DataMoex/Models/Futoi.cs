namespace History_DataMoex.Models;

public sealed record Futoi
{
    public required string SecId { get; init; }
    public required DateTime BeginUtc { get; init; }
    public required DateTime BeginLocal { get; init; }
    public required string Source { get; init; }

    public int? SessId { get; init; }
    public int? SeqNum { get; init; }
    public string? ClGroup { get; init; }

    public long? Pos { get; init; }
    public long? PosLong { get; init; }
    public long? PosShort { get; init; }
    public long? PosLongNum { get; init; }
    public long? PosShortNum { get; init; }
    public string? TradeSessionDate { get; init; }

    public required ulong RowHashV1 { get; init; }
    public required Guid RawObjectId { get; init; }
    public required Guid LoadJobId { get; init; }
}
