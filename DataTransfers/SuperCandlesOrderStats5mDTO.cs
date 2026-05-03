namespace History_DataMoex.DataTransfers
{
    /// <summary>
    /// Одна строка MOEX ALGOPACK OrderStats за 5 минут.
    /// 
    /// OrderStats — статистика заявок.
    /// 
    /// Важно:
    /// это не сделки, а именно поставленные и снятые заявки.
    /// </summary>
    public class SuperCandlesOrderStats5mDTO
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
        /// Для стыковки используем вместе с TradeDate и SecId.
        /// </summary>
        public string? TradeTime { get; init; }

        /// <summary>
        /// Код инструмента.
        /// 
        /// MOEX column: secid
        /// 
        /// Пример:
        /// SBER
        /// </summary>
        public string? SecId { get; init; }

        /// <summary>
        /// Количество поставленных заявок на покупку.
        /// 
        /// MOEX column: put_orders_b
        /// 
        /// put = выставленные заявки,
        /// b = buy.
        /// </summary>
        public int? PutOrdersB { get; init; }

        /// <summary>
        /// Количество поставленных заявок на продажу.
        /// 
        /// MOEX column: put_orders_s
        /// 
        /// s = sell.
        /// </summary>
        public int? PutOrdersS { get; init; }

        /// <summary>
        /// Денежный объём поставленных заявок на покупку.
        /// 
        /// MOEX column: put_val_b
        /// </summary>
        public double? PutValB { get; init; }

        /// <summary>
        /// Денежный объём поставленных заявок на продажу.
        /// 
        /// MOEX column: put_val_s
        /// </summary>
        public double? PutValS { get; init; }

        /// <summary>
        /// Объём поставленных заявок на покупку.
        /// 
        /// MOEX column: put_vol_b
        /// </summary>
        public int? PutVolB { get; init; }

        /// <summary>
        /// Объём поставленных заявок на продажу.
        /// 
        /// MOEX column: put_vol_s
        /// </summary>
        public int? PutVolS { get; init; }

        /// <summary>
        /// Средневзвешенная цена поставленных заявок на покупку.
        /// 
        /// MOEX column: put_vwap_b
        /// </summary>
        public double? PutVwapB { get; init; }

        /// <summary>
        /// Средневзвешенная цена поставленных заявок на продажу.
        /// 
        /// MOEX column: put_vwap_s
        /// </summary>
        public double? PutVwapS { get; init; }

        /// <summary>
        /// Общий объём поставленных заявок.
        /// 
        /// MOEX column: put_vol
        /// 
        /// Покупка + продажа.
        /// </summary>
        public int? PutVol { get; init; }

        /// <summary>
        /// Общая денежная сумма поставленных заявок.
        /// 
        /// MOEX column: put_val
        /// 
        /// Покупка + продажа.
        /// </summary>
        public double? PutVal { get; init; }

        /// <summary>
        /// Общее количество поставленных заявок.
        /// 
        /// MOEX column: put_orders
        /// 
        /// Покупка + продажа.
        /// </summary>
        public int? PutOrders { get; init; }

        /// <summary>
        /// Количество снятых заявок на покупку.
        /// 
        /// MOEX column: cancel_orders_b
        /// </summary>
        public int? CancelOrdersB { get; init; }

        /// <summary>
        /// Количество снятых заявок на продажу.
        /// 
        /// MOEX column: cancel_orders_s
        /// </summary>
        public int? CancelOrdersS { get; init; }

        /// <summary>
        /// Денежный объём снятых заявок на покупку.
        /// 
        /// MOEX column: cancel_val_b
        /// </summary>
        public double? CancelValB { get; init; }

        /// <summary>
        /// Денежный объём снятых заявок на продажу.
        /// 
        /// MOEX column: cancel_val_s
        /// </summary>
        public double? CancelValS { get; init; }

        /// <summary>
        /// Объём снятых заявок на покупку.
        /// 
        /// MOEX column: cancel_vol_b
        /// </summary>
        public int? CancelVolB { get; init; }

        /// <summary>
        /// Объём снятых заявок на продажу.
        /// 
        /// MOEX column: cancel_vol_s
        /// 
        /// Тип в MOEX metadata: int64.
        /// </summary>
        public long? CancelVolS { get; init; }

        /// <summary>
        /// Средневзвешенная цена снятых заявок на покупку.
        /// 
        /// MOEX column: cancel_vwap_b
        /// </summary>
        public double? CancelVwapB { get; init; }

        /// <summary>
        /// Средневзвешенная цена снятых заявок на продажу.
        /// 
        /// MOEX column: cancel_vwap_s
        /// </summary>
        public double? CancelVwapS { get; init; }

        /// <summary>
        /// Общий объём снятых заявок.
        /// 
        /// MOEX column: cancel_vol
        /// 
        /// Покупка + продажа.
        /// </summary>
        public long? CancelVol { get; init; }

        /// <summary>
        /// Общий денежный объём снятых заявок.
        /// 
        /// MOEX column: cancel_val
        /// </summary>
        public double? CancelVal { get; init; }

        /// <summary>
        /// Общее количество снятых заявок.
        /// 
        /// MOEX column: cancel_orders
        /// </summary>
        public long? CancelOrders { get; init; }

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
