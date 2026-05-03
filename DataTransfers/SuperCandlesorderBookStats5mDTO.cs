namespace History_DataMoex.DataTransfers
{
    /// <summary>
    /// Одна строка MOEX ALGOPACK OBStats за 5 минут.
    /// 
    /// OBStats — статистика стакана заявок.
    /// 
    /// Стакан — список текущих заявок на покупку и продажу.
    /// Здесь важны спред, глубина, объёмы bid/ask и дисбаланс.
    /// </summary>
    public record SuperCandlesOrderBookStats5mDTO
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
        /// 
        /// Используется для стыковки с tradestats/orderstats.
        /// </summary>
        public string? TradeTime { get; init; }

        /// <summary>
        /// Код инструмента.
        /// 
        /// MOEX column: secid
        /// </summary>
        public string? SecId { get; init; }

        /// <summary>
        /// Спред между лучшей ценой покупки и лучшей ценой продажи.
        /// 
        /// MOEX column: spread_bbo
        /// 
        /// BBO = Best Bid and Offer.
        /// По-русски: лучшие bid/ask.
        /// 
        /// Спред нужен для оценки стоимости входа в сделку.
        /// </summary>
        public double? SpreadBbo { get; init; }

        /// <summary>
        /// Спред/ширина стакана по 10 уровням.
        /// 
        /// MOEX column: spread_lv10
        /// 
        /// Показывает, насколько широкий стакан глубже первого уровня.
        /// </summary>
        public double? SpreadLv10 { get; init; }

        /// <summary>
        /// Спред/оценка ликвидности на объём 1 млн.
        /// 
        /// MOEX column: spread_1mio
        /// 
        /// Нужен для оценки того, насколько дорого будет набрать крупный объём.
        /// </summary>
        public double? Spread1Mio { get; init; }

        /// <summary>
        /// Количество уровней стакана на покупку.
        /// 
        /// MOEX column: levels_b
        /// 
        /// b = buy/bid.
        /// </summary>
        public int? LevelsB { get; init; }

        /// <summary>
        /// Количество уровней стакана на продажу.
        /// 
        /// MOEX column: levels_s
        /// 
        /// s = sell/ask.
        /// </summary>
        public int? LevelsS { get; init; }

        /// <summary>
        /// Объём заявок на покупку в стакане.
        /// 
        /// MOEX column: vol_b
        /// </summary>
        public long? VolB { get; init; }

        /// <summary>
        /// Объём заявок на продажу в стакане.
        /// 
        /// MOEX column: vol_s
        /// </summary>
        public long? VolS { get; init; }

        /// <summary>
        /// Денежный объём заявок на покупку в стакане.
        /// 
        /// MOEX column: val_b
        /// </summary>
        public long? ValB { get; init; }

        /// <summary>
        /// Денежный объём заявок на продажу в стакане.
        /// 
        /// MOEX column: val_s
        /// </summary>
        public long? ValS { get; init; }

        /// <summary>
        /// Дисбаланс объёма на лучших bid/ask.
        /// 
        /// MOEX column: imbalance_vol_bbo
        /// 
        /// Показывает перевес объёма на лучшем уровне покупки/продажи.
        /// </summary>
        public double? ImbalanceVolBbo { get; init; }

        /// <summary>
        /// Денежный дисбаланс на лучших bid/ask.
        /// 
        /// MOEX column: imbalance_val_bbo
        /// </summary>
        public double? ImbalanceValBbo { get; init; }

        /// <summary>
        /// Общий дисбаланс объёма стакана.
        /// 
        /// MOEX column: imbalance_vol
        /// 
        /// Показывает общий перевес bid или ask по объёму.
        /// </summary>
        public double? ImbalanceVol { get; init; }

        /// <summary>
        /// Общий денежный дисбаланс стакана.
        /// 
        /// MOEX column: imbalance_val
        /// </summary>
        public double? ImbalanceVal { get; init; }

        /// <summary>
        /// Средневзвешенная цена заявок на покупку.
        /// 
        /// MOEX column: vwap_b
        /// 
        /// Это не цена сделок, а цена заявок в стакане.
        /// </summary>
        public double? VwapB { get; init; }

        /// <summary>
        /// Средневзвешенная цена заявок на продажу.
        /// 
        /// MOEX column: vwap_s
        /// </summary>
        public double? VwapS { get; init; }

        /// <summary>
        /// VWAP покупки для объёма 1 млн.
        /// 
        /// MOEX column: vwap_b_1mio
        /// 
        /// Нужен для оценки ликвидности крупной покупки/продажи.
        /// </summary>
        public double? VwapB1Mio { get; init; }

        /// <summary>
        /// VWAP продажи для объёма 1 млн.
        /// 
        /// MOEX column: vwap_s_1mio
        /// </summary>
        public double? VwapS1Mio { get; init; }

        /// <summary>
        /// Системное время формирования/публикации строки.
        /// 
        /// MOEX column: SYSTIME
        /// 
        /// Не использовать как рыночное время.
        /// </summary>
        public DateTime? SysTime { get; init; }
    }
}
