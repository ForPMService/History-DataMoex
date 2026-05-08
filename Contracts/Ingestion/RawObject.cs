namespace History_DataMoex.Contracts.Ingestion
{
    public sealed record RawObject(
        Guid Id,
        string SourceCode,
        string DataNeedCode,
        Guid? InstrumentId,
        string RequestJson,
        string ResponseMetaJson,
        string Sha256,
        string StorageUri,
        long SizeBytes,
        DateTime ReceivedAtUtc);
}