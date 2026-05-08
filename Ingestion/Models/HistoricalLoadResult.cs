using History_DataMoex.Contracts.Ingestion;

namespace History_DataMoex.Ingestion.Models;

public sealed record HistoricalLoadResult(
    Guid JobId,
    JobStatus Status,
    int RowsLoaded,
    int RawObjectsCreated,
    string? Error);