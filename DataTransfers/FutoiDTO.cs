namespace History_DataMoex.DataTransfers
{
    public record FutoiDTO
    {
        /// <summary>
        /// Идентификатор торговой сессии.
        ///
        /// MOEX column: sess_id
        /// </summary>
        public int? SessId { get; init; }

        /// <summary>
        /// Номер последовательности записи.
        ///
        /// MOEX column: seqnum
        /// </summary>
        public int? SeqNum { get; init; }

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
        /// Тикер базового инструмента.
        ///
        /// MOEX column: ticker
        ///
        /// Пример:
        /// Si.
        /// </summary>
        public string? Ticker { get; init; }

        /// <summary>
        /// Группа клиентов.
        ///
        /// MOEX column: clgroup
        ///
        /// Примеры:
        /// FIZ — физические лица,
        /// YUR — юридические лица.
        /// </summary>
        public string? ClGroup { get; init; }

        /// <summary>
        /// Итоговая позиция группы клиентов.
        ///
        /// MOEX column: pos
        /// </summary>
        public long? Pos { get; init; }

        /// <summary>
        /// Длинная позиция.
        ///
        /// MOEX column: pos_long
        /// </summary>
        public long? PosLong { get; init; }

        /// <summary>
        /// Короткая позиция.
        ///
        /// MOEX column: pos_short
        /// </summary>
        public long? PosShort { get; init; }

        /// <summary>
        /// Количество клиентов с длинной позицией.
        ///
        /// MOEX column: pos_long_num
        /// </summary>
        public long? PosLongNum { get; init; }

        /// <summary>
        /// Количество клиентов с короткой позицией.
        ///
        /// MOEX column: pos_short_num
        /// </summary>
        public long? PosShortNum { get; init; }

        /// <summary>
        /// Системное время формирования записи.
        ///
        /// MOEX column: systime
        /// </summary>
        public DateTime? SysTime { get; init; }

        /// <summary>
        /// Дата торговой сессии.
        ///
        /// MOEX column: trade_session_date
        /// </summary>
        public string? TradeSessionDate { get; init; }
    }
}
