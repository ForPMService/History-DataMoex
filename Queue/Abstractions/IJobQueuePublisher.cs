using History_DataMoex.Queue.Models;

namespace History_DataMoex.Queue.Abstractions;

/// <summary>
/// Публикует короткое сообщение о задаче в оперативную очередь.
/// PostgreSQL остаётся источником истины по задаче.
/// Будущая реализация: Redis Streams.
/// </summary>
public interface IJobQueuePublisher
{
    Task PublishAsync(JobQueueMessage message, CancellationToken ct);
}