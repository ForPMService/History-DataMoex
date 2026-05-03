namespace History_DataMoex.DataTransfers
{
    /// <summary>
    /// Информация о пагинации MOEX.
    /// 
    /// Нужна, чтобы понять:
    /// сколько всего строк есть,
    /// сколько строк пришло на одной странице,
    /// с какого индекса загружать следующую страницу.
    /// </summary>
    public record PaginationCursorDTO
    {
        /// <summary>
        /// Начальный индекс текущей страницы.
        /// 
        /// MOEX column: INDEX
        /// 
        /// Пример:
        /// 0, 100, 200.
        /// </summary>
        public int? Index { get; init; }

        /// <summary>
        /// Общее количество строк в ответе по запросу.
        /// 
        /// MOEX column: TOTAL
        /// 
        /// Если Total больше PageSize, нужно догружать следующие страницы.
        /// </summary>
        public int? Total { get; init; }

        /// <summary>
        /// Размер страницы.
        /// 
        /// MOEX column: PAGESIZE
        /// 
        /// Сколько строк MOEX отдаёт за один запрос.
        /// </summary>
        public int? PageSize { get; init; }
    }
}
