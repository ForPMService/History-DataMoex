namespace History_DataMoex.Ingestion.Pipeline;

public interface ILoadJobRunner
{
    Task RunOnceAsync(CancellationToken ct);
}