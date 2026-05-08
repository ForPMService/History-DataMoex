namespace History_DataMoex.Contracts.Ingestion
{
    public sealed record NormalizedRows<T>(
        string DataNeedCode,
        Guid InstrumentId,
        string SourceCode,
        Guid RawObjectId,
        Guid JobId,
        IReadOnlyList<T> Rows,
        string IdempotencyKey);
}