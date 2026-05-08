namespace History_DataMoex.Contracts.Pagination;

/// <summary>
/// Запрос следующей страницы к MOEX.
/// Start соответствует query-параметру start.
/// </summary>
public sealed record MoexPageRequest(
    int Start,
    MoexPaginationKind Kind);