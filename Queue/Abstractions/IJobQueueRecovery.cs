namespace History_DataMoex.Queue.Abstractions;

/// <summary>
/// Восстановление очереди после потери Redis.
/// В будущей реализации будет читать runnable jobs из PostgreSQL
/// и повторно публиковать их в Redis Streams.
/// TODO: retry_pending, stale jobs и Dead Letter Queue будут описаны отдельной задачей реализации.
/// </summary>
public interface IJobQueueRecovery
{
    Task RecoverAsync(CancellationToken ct);
}