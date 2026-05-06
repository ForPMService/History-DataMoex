namespace History_DataMoex.DataTransfers
{
    public record SuperCandlesFuturesOrderBookStats5mDTO
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
        ///
        /// Пример:
        /// SiM5, BRM5, GDM5.
        /// </summary>
        public string? SecId { get; init; }

        /// <summary>
        /// Код базового актива.
        ///
        /// MOEX column: asset_code
        ///
        /// Например:
        /// Si, BR, GD.
        /// </summary>
        public string? AssetCode { get; init; }

        /// <summary>
        /// Средняя цена между лучшей ценой покупки и лучшей ценой продажи.
        ///
        /// MOEX column: mid_price
        /// </summary>
        public double? MidPrice { get; init; }

        /// <summary>
        /// Микроцена с учётом дисбаланса объёмов в стакане.
        ///
        /// MOEX column: micro_price
        /// </summary>
        public double? MicroPrice { get; init; }

        /// <summary>
        /// Спред по первому уровню стакана.
        ///
        /// MOEX column: spread_l1
        /// </summary>
        public double? SpreadL1 { get; init; }

        /// <summary>
        /// Спред по двум уровням стакана.
        ///
        /// MOEX column: spread_l2
        /// </summary>
        public double? SpreadL2 { get; init; }

        /// <summary>
        /// Спред по трём уровням стакана.
        ///
        /// MOEX column: spread_l3
        /// </summary>
        public double? SpreadL3 { get; init; }

        /// <summary>
        /// Спред по пяти уровням стакана.
        ///
        /// MOEX column: spread_l5
        /// </summary>
        public double? SpreadL5 { get; init; }

        /// <summary>
        /// Спред по десяти уровням стакана.
        ///
        /// MOEX column: spread_l10
        /// </summary>
        public double? SpreadL10 { get; init; }

        /// <summary>
        /// Спред по двадцати уровням стакана.
        ///
        /// MOEX column: spread_l20
        /// </summary>
        public double? SpreadL20 { get; init; }

        /// <summary>
        /// Количество доступных уровней на стороне покупки.
        ///
        /// MOEX column: levels_b
        /// </summary>
        public int? LevelsB { get; init; }

        /// <summary>
        /// Количество доступных уровней на стороне продажи.
        ///
        /// MOEX column: levels_s
        /// </summary>
        public int? LevelsS { get; init; }

        /// <summary>
        /// Объём заявок на покупку на первом уровне стакана.
        ///
        /// MOEX column: vol_b_l1
        /// </summary>
        public long? VolBL1 { get; init; }

        /// <summary>
        /// Объём заявок на покупку по двум уровням стакана.
        ///
        /// MOEX column: vol_b_l2
        /// </summary>
        public long? VolBL2 { get; init; }

        /// <summary>
        /// Объём заявок на покупку по трём уровням стакана.
        ///
        /// MOEX column: vol_b_l3
        /// </summary>
        public long? VolBL3 { get; init; }

        /// <summary>
        /// Объём заявок на покупку по пяти уровням стакана.
        ///
        /// MOEX column: vol_b_l5
        /// </summary>
        public long? VolBL5 { get; init; }

        /// <summary>
        /// Объём заявок на покупку по десяти уровням стакана.
        ///
        /// MOEX column: vol_b_l10
        /// </summary>
        public long? VolBL10 { get; init; }

        /// <summary>
        /// Объём заявок на покупку по двадцати уровням стакана.
        ///
        /// MOEX column: vol_b_l20
        /// </summary>
        public long? VolBL20 { get; init; }

        /// <summary>
        /// Объём заявок на продажу на первом уровне стакана.
        ///
        /// MOEX column: vol_s_l1
        /// </summary>
        public long? VolSL1 { get; init; }

        /// <summary>
        /// Объём заявок на продажу по двум уровням стакана.
        ///
        /// MOEX column: vol_s_l2
        /// </summary>
        public long? VolSL2 { get; init; }

        /// <summary>
        /// Объём заявок на продажу по трём уровням стакана.
        ///
        /// MOEX column: vol_s_l3
        /// </summary>
        public long? VolSL3 { get; init; }

        /// <summary>
        /// Объём заявок на продажу по пяти уровням стакана.
        ///
        /// MOEX column: vol_s_l5
        /// </summary>
        public long? VolSL5 { get; init; }

        /// <summary>
        /// Объём заявок на продажу по десяти уровням стакана.
        ///
        /// MOEX column: vol_s_l10
        /// </summary>
        public long? VolSL10 { get; init; }

        /// <summary>
        /// Объём заявок на продажу по двадцати уровням стакана.
        ///
        /// MOEX column: vol_s_l20
        /// </summary>
        public long? VolSL20 { get; init; }

        /// <summary>
        /// VWAP покупки по трём уровням стакана.
        ///
        /// MOEX column: vwap_b_l3
        /// </summary>
        public double? VwapBL3 { get; init; }

        /// <summary>
        /// VWAP покупки по пяти уровням стакана.
        ///
        /// MOEX column: vwap_b_l5
        /// </summary>
        public double? VwapBL5 { get; init; }

        /// <summary>
        /// VWAP покупки по десяти уровням стакана.
        ///
        /// MOEX column: vwap_b_l10
        /// </summary>
        public double? VwapBL10 { get; init; }

        /// <summary>
        /// VWAP покупки по двадцати уровням стакана.
        ///
        /// MOEX column: vwap_b_l20
        /// </summary>
        public double? VwapBL20 { get; init; }

        /// <summary>
        /// VWAP продажи по трём уровням стакана.
        ///
        /// MOEX column: vwap_s_l3
        /// </summary>
        public double? VwapSL3 { get; init; }

        /// <summary>
        /// VWAP продажи по пяти уровням стакана.
        ///
        /// MOEX column: vwap_s_l5
        /// </summary>
        public double? VwapSL5 { get; init; }

        /// <summary>
        /// VWAP продажи по десяти уровням стакана.
        ///
        /// MOEX column: vwap_s_l10
        /// </summary>
        public double? VwapSL10 { get; init; }

        /// <summary>
        /// VWAP продажи по двадцати уровням стакана.
        ///
        /// MOEX column: vwap_s_l20
        /// </summary>
        public double? VwapSL20 { get; init; }

        /// <summary>
        /// Системное время формирования записи на стороне MOEX.
        ///
        /// MOEX column: SYSTIME
        /// </summary>
        public DateTime? SysTime { get; init; }
    }
}
