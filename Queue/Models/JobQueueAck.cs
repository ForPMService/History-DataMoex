namespace History_DataMoex.Queue.Models;

/// <summary>
/// Результат подтверждения обработки сообщения очереди.
/// В будущей реализации будет связан с Redis Streams ACK.
/// </summary>
public sealed record JobQueueAck(
    Guid JobId,
    string MessageId,
    DateTimeOffset AckedAtUtc);