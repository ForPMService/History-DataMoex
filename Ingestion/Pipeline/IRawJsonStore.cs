namespace History_DataMoex.Ingestion.Pipeline;

public interface IRawJsonStore
{
    Task<Guid> SaveAsync(
        string sourceCode,
        string requestUrl,
        Stream rawBody,
        CancellationToken ct);
}