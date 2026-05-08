namespace History_DataMoex.Contracts.Ingestion
{
    public sealed record RateLimits(
        int RequestsPerMinute,
        int BurstSize);
}