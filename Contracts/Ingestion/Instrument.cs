using History_DataMoex.Normalization.Models;

namespace History_DataMoex.Contracts.Ingestion
{
    public sealed record Instrument(
        Guid InstrumentId,
        string DisplayCode,
        string SecId,
        string BoardId,
        string? AssetCode,
        string? Isin,
        InstrumentType Type,
        bool IsActive);
}