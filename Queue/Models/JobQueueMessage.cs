namespace History_DataMoex.Queue.Models;

/// <summary>
/// Минимальное сообщение, которое будет публиковаться в Redis Streams.
/// Полные параметры задачи хранятся в PostgreSQL, а не в Redis.
/// CorrelationId в C# хранится как Guid; при будущей сериализации в Redis будет преобразован в string.
/// </summary>
public sealed record JobQueueMessage(
    Guid JobId,
    string JobType,
    string SourceCode,
    string DataNeedCode,
    Guid CorrelationId,
    DateTimeOffset EnqueuedAtUtc);