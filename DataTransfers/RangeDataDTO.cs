namespace History_DataMoex.DataTransfers
{
    /// <summary>
    /// Диапазон дат, который возвращает MOEX в служебном блоке dates.
    /// 
    /// Это не торговая строка, а справочная информация источника.
    /// </summary>
    public class RangeDataDTO
    {
        /// <summary>
        /// Начальная дата доступного диапазона.
        /// 
        /// MOEX column: from
        /// </summary>
        public string? From { get; init; }

        /// <summary>
        /// Конечная дата доступного диапазона.
        /// 
        /// MOEX column: till
        /// </summary>
        public string? Till { get; init; }
    }
}
