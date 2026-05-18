using History_DataMoex.Contracts.Dto;

namespace History_DataMoex.RawCapture;

/// <summary>
/// Результат вызова internal raw метода для multi-table endpoint:
/// FuturesSecurities (Forts+Options) — без cursor;
/// Stock/Futures session (sessions+types) — без cursor;
/// Suspended+Reasons — с cursor;
/// SecurityChanges+Attributes — с cursor.
///
/// MainItems / SecondaryItems — две связанные коллекции из одного HTTP-ответа.
/// NextCursor — null для непагинируемых multi-table, не null для пагинируемых.
///
/// Multi-table cohesion (Lock §6): MainItems и SecondaryItems получают ОДНУ И ТУ ЖЕ RawObjectId —
/// обе таблицы пришли из одного HTTP-ответа.
/// </summary>
public sealed record SourceMultiTable<T1, T2>
{
    public required IReadOnlyList<T1> MainItems { get; init; }
    public required IReadOnlyList<T2> SecondaryItems { get; init; }
    public PaginationCursorDTO? NextCursor { get; init; }
    public required Guid RawObjectId { get; init; }
    public required DateTime FetchedAtUtc { get; init; }
}
