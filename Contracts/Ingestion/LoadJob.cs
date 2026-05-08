namespace History_DataMoex.Contracts.Ingestion
{
    public sealed record LoadJob(
        Guid Id,
        string SourceCode,
        string DataNeedCode,
        Guid? InstrumentId,
        DateTime PeriodFromUtc,
        DateTime PeriodTillUtc,
        JobStatus Status,
        int Attempts,
        string? LastError,
        Guid CorrelationId,
        DateTime CreatedAtUtc,
        DateTime? StartedAtUtc,
        DateTime? FinishedAtUtc);
}