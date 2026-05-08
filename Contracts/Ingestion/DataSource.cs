namespace History_DataMoex.Contracts.Ingestion
{
    public sealed record DataSource(
        string SourceCode,
        string DisplayName,
        string BaseUrl,
        AuthType AuthType,
        RateLimits Limits);
}