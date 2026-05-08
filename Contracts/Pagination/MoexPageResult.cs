namespace History_DataMoex.Contracts.Pagination;

/// <summary>
/// Результат одной страницы данных MOEX.
/// Для Cursor-стратегии Index/Total/PageSize берутся из MOEX cursor-блока.
/// Для FixedPage500 и FixedPage1000 эти поля могут быть null.
/// </summary>
/// <param name="Rows">
/// Строки данных, полученные на текущей странице.
/// </param>
/// <param name="Kind">
/// Тип стратегии пагинации MOEX.
/// </param>
/// <param name="Index">
/// Начальный индекс текущей страницы. Для Cursor берётся из INDEX.
/// Для fixed-page стратегий можно передавать текущий start.
/// </param>
/// <param name="Total">
/// Общее количество строк. Обычно известно только для Cursor-стратегии.
/// </param>
/// <param name="PageSize">
/// Размер страницы. Обычно известен только для Cursor-стратегии.
/// </param>
/// <param name="HasMore">
/// Информационный флаг: есть ли следующая страница по мнению вызывающего кода.
/// Окончательное решение о следующем запросе принимает MoexPaginationDecision.Next().
/// </param>
public sealed record MoexPageResult<T>(
    IReadOnlyList<T> Rows,
    MoexPaginationKind Kind,
    int? Index,
    int? Total,
    int? PageSize,
    bool HasMore);