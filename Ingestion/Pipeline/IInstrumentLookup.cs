namespace History_DataMoex.Ingestion.Pipeline;

public interface IInstrumentLookup
{
    Task<Guid?> ResolveAsync(
        string secId,
        string? boardId,
        string? assetCode,
        CancellationToken ct);
}