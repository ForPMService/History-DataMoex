namespace History_DataMoex.DataTransfers
{
    public class SuperCandlesFuturesTradeStats5mDTO
    {
        /// <summary>
        /// Торговая дата.
        ///
        /// MOEX column: tradedate
        /// </summary>
        public string? TradeDate { get; init; }

        /// <summary>
        /// Время торговой точки.
        ///
        /// MOEX column: tradetime
        /// </summary>
        public string? TradeTime { get; init; }

        /// <summary>
        /// Код инструмента.
        ///
        /// MOEX column: secid
        /// </summary>
        public string? SecId { get; init; }

        /// <summary>
        /// Код базового актива.
        ///
        /// MOEX column: asset_code
        /// </summary>
        public string? AssetCode { get; init; }

        /// <summary>
        /// Цена открытия за интервал.
        ///
        /// MOEX column: pr_open
        /// </summary>
        public double? PrOpen { get; init; }

        /// <summary>
        /// Максимальная цена за интервал.
        ///
        /// MOEX column: pr_high
        /// </summary>
        public double? PrHigh { get; init; }

        /// <summary>
        /// Минимальная цена за интервал.
        ///
        /// MOEX column: pr_low
        /// </summary>
        public double? PrLow { get; init; }

        /// <summary>
        /// Цена закрытия за интервал.
        ///
        /// MOEX column: pr_close
        /// </summary>
        public double? PrClose { get; init; }

        /// <summary>
        /// Стандартное отклонение цены внутри интервала.
        ///
        /// MOEX column: pr_std
        /// </summary>
        public double? PrStd { get; init; }

        /// <summary>
        /// Объём сделок в контрактах.
        ///
        /// MOEX column: vol
        /// </summary>
        public long? Vol { get; init; }

        /// <summary>
        /// Оборот.
        ///
        /// MOEX column: val
        /// </summary>
        public long? Val { get; init; }

        /// <summary>
        /// Количество сделок.
        ///
        /// MOEX column: trades
        /// </summary>
        public int? Trades { get; init; }

        /// <summary>
        /// VWAP за интервал.
        ///
        /// MOEX column: pr_vwap
        /// </summary>
        public double? PrVwap { get; init; }

        /// <summary>
        /// Изменение цены.
        ///
        /// MOEX column: pr_change
        /// </summary>
        public double? PrChange { get; init; }

        /// <summary>
        /// Количество сделок, классифицированных как покупки.
        ///
        /// MOEX column: trades_b
        /// </summary>
        public int? TradesB { get; init; }

        /// <summary>
        /// Количество сделок, классифицированных как продажи.
        ///
        /// MOEX column: trades_s
        /// </summary>
        public int? TradesS { get; init; }

        /// <summary>
        /// Оборот покупок.
        ///
        /// MOEX column: val_b
        /// </summary>
        public double? ValB { get; init; }

        /// <summary>
        /// Оборот продаж.
        ///
        /// MOEX column: val_s
        /// </summary>
        public double? ValS { get; init; }

        /// <summary>
        /// Объём покупок в контрактах.
        ///
        /// MOEX column: vol_b
        /// </summary>
        public long? VolB { get; init; }

        /// <summary>
        /// Объём продаж в контрактах.
        ///
        /// MOEX column: vol_s
        /// </summary>
        public long? VolS { get; init; }

        /// <summary>
        /// Дисбаланс buy/sell.
        ///
        /// MOEX column: disb
        /// </summary>
        public double? Disb { get; init; }

        /// <summary>
        /// VWAP покупок.
        ///
        /// MOEX column: pr_vwap_b
        /// </summary>
        public double? PrVwapB { get; init; }

        /// <summary>
        /// VWAP продаж.
        ///
        /// MOEX column: pr_vwap_s
        /// </summary>
        public double? PrVwapS { get; init; }

        /// <summary>
        /// Гарантийное обеспечение.
        ///
        /// MOEX column: im
        /// </summary>
        public double? Im { get; init; }

        /// <summary>
        /// Открытый интерес на начало интервала.
        ///
        /// MOEX column: oi_open
        /// </summary>
        public long? OiOpen { get; init; }

        /// <summary>
        /// Максимальный открытый интерес внутри интервала.
        ///
        /// MOEX column: oi_high
        /// </summary>
        public long? OiHigh { get; init; }

        /// <summary>
        /// Минимальный открытый интерес внутри интервала.
        ///
        /// MOEX column: oi_low
        /// </summary>
        public long? OiLow { get; init; }

        /// <summary>
        /// Открытый интерес на конец интервала.
        ///
        /// MOEX column: oi_close
        /// </summary>
        public long? OiClose { get; init; }

        /// <summary>
        /// Секундная цена открытия.
        ///
        /// MOEX column: sec_pr_open
        /// </summary>
        public int? SecPrOpen { get; init; }

        /// <summary>
        /// Секундная максимальная цена.
        ///
        /// MOEX column: sec_pr_high
        /// </summary>
        public int? SecPrHigh { get; init; }

        /// <summary>
        /// Секундная минимальная цена.
        ///
        /// MOEX column: sec_pr_low
        /// </summary>
        public int? SecPrLow { get; init; }

        /// <summary>
        /// Секундная цена закрытия.
        ///
        /// MOEX column: sec_pr_close
        /// </summary>
        public int? SecPrClose { get; init; }

        /// <summary>
        /// Системное время формирования записи на стороне MOEX.
        ///
        /// MOEX column: SYSTIME
        /// </summary>
        public DateTime? SysTime { get; init; }
    }
}
