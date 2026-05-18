using History_DataMoex.Contracts.Dto;

namespace History_DataMoex.RawCapture;

/// <summary>
/// Результат вызова internal raw метода для single-table endpoint.
/// Items — внутренние модели одного типа.
/// NextCursor — для cursor pagination; null если страница последняя или endpoint не пагинируется.
/// RawObjectId — идентификатор raw payload, сохранённого через IRawObjectStore.SaveAsync ДО парсинга.
/// Тот же RawObjectId проброшен в каждую модель в Items через MapContext (Lock §6).
/// FetchedAtUtc — момент HTTP-вызова, проброшен в каждую модель.
/// </summary>
public sealed record SourcePage<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public PaginationCursorDTO? NextCursor { get; init; }
    public required Guid RawObjectId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
