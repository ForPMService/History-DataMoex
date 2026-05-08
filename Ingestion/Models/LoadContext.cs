namespace History_DataMoex.Ingestion.Models;

public sealed record LoadContext(
    string SourceCode,
    Guid InstrumentId,
    Guid RawObjectId,
    Guid JobId,
    Guid CorrelationId,
    DateTimeOffset NowUtc);