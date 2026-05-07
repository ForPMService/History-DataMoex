namespace History_DataMoex.DataTransfers
{
    public record Hi2FuturesDTO
    {
        /// <summary>
        /// Торговая дата.
        ///
        /// MOEX column: tradedate
        /// </summary>
        public string? TradeDate { get; init; }

        /// <summary>
        /// Торговое время.
        ///
        /// MOEX column: tradetime
        /// </summary>
        public string? TradeTime { get; init; }

        /// <summary>
        /// Код срочного инструмента.
        ///
        /// MOEX column: secid
        ///
        /// Пример:
        /// SiM6.
        /// </summary>
        public string? SecId { get; init; }

        /// <summary>
        /// Код базового актива.
        ///
        /// MOEX column: asset_code
        ///
        /// Пример:
        /// Si.
        /// </summary>
        public string? AssetCode { get; init; }

        /// <summary>
        /// Название метрики HI2.
        ///
        /// MOEX column: metric
        ///
        /// Примеры:
        /// hhi_agressive,
        /// hhi_agressive_buy,
        /// hhi_agressive_sell,
        /// hhi_buy,
        /// hhi_sell,
        /// hhi_volume.
        /// </summary>
        public string? Metric { get; init; }

        /// <summary>
        /// Значение метрики.
        ///
        /// MOEX column: value
        /// </summary>
        public double? Value { get; init; }

        /// <summary>
        /// Справочная информация по метрике.
        ///
        /// MOEX column: reference
        ///
        /// В примере приходит пустая строка.
        /// </summary>
        public string? Reference { get; init; }

        /// <summary>
        /// Системное время формирования записи.
        ///
        /// MOEX column: SYSTIME
        /// </summary>
        public DateTime? SysTime { get; init; }
    }
}
