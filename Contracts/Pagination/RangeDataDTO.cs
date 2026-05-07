namespace History_DataMoex.Contracts.Pagination
{
    /// <summary>
    /// Диапазон дат, который возвращает MOEX в служебном блоке dates.
    /// 
    /// Это не торговая строка, а справочная информация источника.
    /// </summary>
    public record RangeDataDTO
    {
        /// <summary>
        /// Начальная дата доступного диапазона.
        /// 
        /// MOEX столбец: from
        /// </summary>
        public string? From { get; init; }

        /// <summary>
        /// Конечная дата доступного диапазона.
        /// 
        /// MOEX столбец: till
        /// </summary>
        public string? Till { get; init; }
    }
}