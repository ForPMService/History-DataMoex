namespace History_DataMoex.DataTransfers.Calendar
{
    /// <summary>
    /// Опционная серия — справочник серий и экспираций.
    ///
    /// Источник: GET /iss/calendars/futures/securities.json
    /// Таблица: options
    ///
    /// Одна строка — одна опционная серия.
    ///
    /// Пример:
    /// "Акции", ALRS, ALRSP190630XE, Q, E, P, "Опцион на ... АЛРОСА", 2030-06-19.
    /// </summary>
    public record CalendarOptionsSeriesDTO
    {
        /// <summary>
        /// Тип базового актива (человекочитаемое название).
        ///
        /// MOEX column: asset_type_name
        /// MOEX type: string
        ///
        /// Пример: "Акции", "Валюта", "Индекс".
        /// </summary>
        public string? AssetTypeName { get; init; }

        /// <summary>
        /// Код базового актива.
        ///
        /// MOEX column: asset_code
        /// MOEX type: string
        ///
        /// Пример: ALRS, SBER, Si.
        /// </summary>
        public string? AssetCode { get; init; }

        /// <summary>
        /// Название серии (код серии).
        ///
        /// MOEX column: series_name
        /// MOEX type: string
        ///
        /// Пример: ALRSP190630XE.
        /// </summary>
        public string? SeriesName { get; init; }

        /// <summary>
        /// Тип серии.
        ///
        /// MOEX column: series_type
        /// MOEX type: string
        ///
        /// Пример: "Q" — квартальная (?).
        /// </summary>
        public string? SeriesType { get; init; }

        /// <summary>
        /// Тип исполнения.
        ///
        /// MOEX column: exec_type
        /// MOEX type: string
        ///
        /// "E" — европейский (?), "A" — американский (?).
        /// </summary>
        public string? ExecType { get; init; }

        /// <summary>
        /// Стиль маржирования.
        ///
        /// MOEX column: margin_style
        /// MOEX type: string
        ///
        /// "P" — premium style (?).
        /// </summary>
        public string? MarginStyle { get; init; }

        /// <summary>
        /// Полное название контракта.
        ///
        /// MOEX column: contract_name
        /// MOEX type: string
        ///
        /// Пример: "Опцион на обыкновенные акции АК «АЛРОСА» (ПАО)".
        /// </summary>
        public string? ContractName { get; init; }

        /// <summary>
        /// Дата экспирации серии.
        ///
        /// MOEX column: expiration_date
        /// MOEX type: date
        ///
        /// Пример: 2030-06-19.
        /// </summary>
        public string? ExpirationDate { get; init; }

        /// <summary>
        /// Тип экспирации.
        ///
        /// MOEX column: expiration_type
        /// MOEX type: string
        ///
        /// "tc" — trading close (?).
        /// </summary>
        public string? ExpirationType { get; init; }

        /// <summary>
        /// Время экспирации.
        ///
        /// MOEX column: expiration_time
        /// MOEX type: time
        ///
        /// Приходит как строка "HH:mm:ss".
        /// Пример: 19:00:00.
        /// </summary>
        public string? ExpirationTime { get; init; }

        /// <summary>
        /// Доступен ли в weekend-сессии.
        ///
        /// MOEX column: weekend_session
        /// MOEX type: int32
        ///
        /// 0 — нет, 1 — да.
        /// </summary>
        public int? WeekendSession { get; init; }
    }
}
