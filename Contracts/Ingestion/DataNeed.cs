namespace History_DataMoex.Contracts.Ingestion
{
    public sealed record DataNeed(
        string DataNeedCode,
        string BusinessPurpose,
        NeedPriority Priority,
        IReadOnlyList<string> SourceCandidates,
        ResearchStatus ResearchStatus,
        string StorageTarget,
        string CanonicalModel);
}