namespace History_DataMoex.Queue.Models;

/// <summary>
/// Состояние чтения очереди worker'ом.
/// В будущей реализации будет использоваться для consumer group Redis Streams.
/// </summary>
public sealed record QueueConsumerState(
    string StreamName,
    string ConsumerGroup,
    string ConsumerName,
    DateTimeOffset CheckedAtUtc);