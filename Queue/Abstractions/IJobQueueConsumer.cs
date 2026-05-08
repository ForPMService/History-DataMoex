using History_DataMoex.Queue.Models;

namespace History_DataMoex.Queue.Abstractions;

/// <summary>
/// Читает сообщения из оперативной очереди задач.
/// Будущая реализация: Redis Streams consumer group.
/// </summary>
public interface IJobQueueConsumer
{
    Task<IReadOnlyList<JobQueueMessage>> ReadAsync(
        int maxCount,
        TimeSpan blockFor,
        CancellationToken ct);

    Task AckAsync(JobQueueAck ack, CancellationToken ct);
}