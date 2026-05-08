namespace History_DataMoex.Ingestion.Models;

public sealed record HistoricalLoadRequest(
    string SourceCode,
    string DataNeedCode,
    string SecId,
    string? BoardId,
    DateOnly From,
    DateOnly Till,
    Guid CorrelationId);