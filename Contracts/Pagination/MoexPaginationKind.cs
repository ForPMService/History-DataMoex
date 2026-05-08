namespace History_DataMoex.Contracts.Pagination;

/// <summary>
/// Тип пагинации, который диктует источник MOEX.
/// Это не наш внутренний выбор, а отражение разных форматов ответа MOEX.
/// </summary>
public enum MoexPaginationKind
{
    /// <summary>
    /// MOEX отдаёт служебный cursor-блок с INDEX, TOTAL, PAGESIZE.
    /// Используется для большинства ALGOPACK endpoint-ов.
    /// </summary>
    Cursor,

    /// <summary>
    /// MOEX не отдаёт cursor-блок, но candles догружаются через start
    /// с фактическим размером страницы 500 строк.
    /// </summary>
    FixedPage500,

    /// <summary>
    /// MOEX не отдаёт cursor-блок, но FUTOI догружается через start
    /// с фактическим размером страницы 1000 строк.
    /// </summary>
    FixedPage1000
}