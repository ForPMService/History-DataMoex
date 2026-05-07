namespace History_DataMoex.DataTransfers
{
    public record MegaAlertsFuturesDTO
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
        /// Тип алерта.
        ///
        /// MOEX column: alert_type
        ///
        /// Примеры:
        /// pr_change_99_9_pctl-,
        /// pr_low_min,
        /// vol_99_9_pctl,
        /// vol_s_99_9_pctl,
        /// net_vol_99_9_pctl-,
        /// oi_close_change_99_9_pctl-.
        /// </summary>
        public string? AlertType { get; init; }

        /// <summary>
        /// Пороговое значение, при превышении которого сработал алерт.
        ///
        /// MOEX column: threshold
        /// </summary>
        public double? Threshold { get; init; }

        /// <summary>
        /// Фактическое значение показателя.
        ///
        /// MOEX column: value
        /// </summary>
        public double? Value { get; init; }

        /// <summary>
        /// Справочная информация по алерту.
        ///
        /// MOEX column: reference
        ///
        /// В ответе приходит строка, внутри которой находится JSON.
        /// На этом уровне оставляем как string.
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
