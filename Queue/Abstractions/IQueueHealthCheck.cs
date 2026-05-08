using History_DataMoex.Queue.Models;

namespace History_DataMoex.Queue.Abstractions;

/// <summary>
/// Проверка доступности оперативной очереди.
/// Это не ASP.NET Health Check, а доменная абстракция для будущего Redis Streams adapter.
/// </summary>
public interface IQueueHealthCheck
{
    Task<QueueConsumerState> GetStateAsync(CancellationToken ct);
}