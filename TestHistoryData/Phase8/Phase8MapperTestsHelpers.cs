using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Mappers;

namespace TestHistoryData.Phase8;

/// <summary>
/// Тестовые фабрики для Phase 8 мапперов. Создают валидные DTO с дефолтными значениями
/// и MapContext с заполненными lineage полями. Применяются smoke тестами 8-B и
/// полным покрытием 8-D.
/// </summary>
public static class Phase8MapperTestsHelpers
{
    /// <summary>
    /// Создаёт MapContext с дефолтными lineage полями. sourceCode по умолчанию MOEX_CALENDAR,
    /// для ISS тестов передавать "MOEX_ISS" явно.
    /// FetchedAtUtc — фиксированное время (2026-05-18 12:00 UTC) для воспроизводимости hash-тестов 8-D.
    /// </summary>
    public static MapContext MakeContext(string sourceCode = "MOEX_CALENDAR") => new(
        SourceCode: sourceCode,
        Endpoint: "/test/endpoint.json",
        SourceTimezone: "Europe/Moscow",
        LoadJobId: Guid.CreateVersion7(),
        RawObjectId: Guid.CreateVersion7(),
        FetchedAtUtc: new DateTime(2026, 5, 18, 12, 0, 0, DateTimeKind.Utc));

    // ===== ISS factories =====

    public static StockSecurityDTO MakeValidIssStockSecurityDto(
        string secId = "SBER",
        string boardId = "TQBR")
        => new()
        {
            SECID = secId,
            BOARDID = boardId,
            SHORTNAME = "Сбербанк",
            SECNAME = "Сбербанк ПАО ао",
            MARKETCODE = "FNDT",
            PREVLEGALCLOSEPRICE = 290.5m,
            LOTSIZE = 10,
            FACEVALUE = 3.0,
            PREVDATE = new DateTime(2026, 5, 17),
        };

    public static FuturesSecurityDTO MakeValidIssFuturesSecurityDto(string secId = "SiM6")
        => new()
        {
            SECID = secId,
            SHORTNAME = "Si-6.26",
            SECNAME = "USD/RUB FUTURES",
            ASSETCODE = "Si",
            INITIALMARGIN = 5000.0,
            PREVSETTLEPRICE = 92450.0,
            MINSTEP = 1.0,
            HIGHLIMIT = 95000.0,
            LOWLIMIT = 90000.0,
            STEPPRICE = 1.0,
            PREVPRICE = 92500.0,
            DECIMALS = 0,
            LOTVOLUME = 1000,
            PREVOPENPOSITION = 250000L,
            LASTTRADEDATE = new DateTime(2026, 6, 18),
            LASTDELDATE = new DateTime(2026, 6, 19),
        };

    // ===== Calendar Group A factories =====

    public static CalendarFortsContractDTO MakeValidCalendarFortsContractDto(string secId = "SiM6")
        => new()
        {
            SecId = secId,
            AssetCode = "Si",
            ShortName = "Si-6.26",
            ExecType = "FX",
            ContractName = "USD/RUB June 2026",
            ExpirationDate = "2026-06-18",
            EndDate = "2026-06-18",
            ExpirationType = "Standard",
            ExpirationTime = "18:45:00",
            WeekendSession = 0,
        };

    public static CalendarSuspendedDTO MakeValidCalendarSuspensionDto(string secId = "SBER")
        => new()
        {
            SecId = secId,
            ReasonId = "1",
            DateFrom = "2026-05-10",
            DateTill = "2026-05-15",
            BoardId = "TQBR",
            SettleCodes = "Y2,Y3",
            ChangeDate = "2026-05-09",
            UpdateTime = new DateTime(2026, 5, 9, 18, 0, 0, DateTimeKind.Unspecified),
        };

    public static CalendarSecurityChangeDTO MakeValidCalendarSecurityChangeDto(string secId = "SBER")
        => new()
        {
            UpdateTime = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Unspecified),
            Action = "UPDATE",
            SecId = secId,
            AttributeName = "LOTSIZE",
            BeforeValue = "10",
            AfterValue = "1",
        };

    // ===== Calendar Group B factories =====

    public static CalendarStockSessionDTO MakeValidCalendarStockSessionDto(string? secId = "-")
        => new()
        {
            TradeDate = "2026-05-17",
            TradingSession = 2,
            BoardId = "TQBR",
            SecId = secId,
            Type = "MAIN",
            TimeFrom = "10:00:00",
            TimeTill = "18:45:00",
            UpdateTime = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Unspecified),
        };

    public static CalendarFuturesSessionDTO MakeValidCalendarFuturesSessionDto(string? secId = "-")
        => new()
        {
            TradeSessionDate = "2026-05-17",
            BoardId = "RFUD",
            SecId = secId,
            Type = "MAIN",
            TimeFrom = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Unspecified),
            TimeTill = new DateTime(2026, 5, 17, 18, 50, 0, DateTimeKind.Unspecified),
            UpdateTime = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Unspecified),
        };

    // ===== Calendar Group C factories =====

    public static CalendarOffDaysAllDTO MakeValidCalendarOffDayAllDto()
        => new()
        {
            TradeDate = "2026-05-09",
            CurrencyWorkday = 0,
            CurrencyTradeSessionDate = "2026-05-08",
            CurrencyReason = "H",
            FuturesWorkday = 0,
            FuturesTradeSessionDate = "2026-05-08",
            FuturesReason = "H",
            StockWorkday = 0,
            StockTradeSessionDate = "2026-05-08",
            StockReason = "H",
        };

    public static CalendarOptionsSeriesDTO MakeValidCalendarOptionsSeriesDto()
        => new()
        {
            AssetTypeName = "Currency",
            AssetCode = "Si",
            SeriesName = "Si-6.26",
            SeriesType = "Standard",
            ExecType = "FX",
            MarginStyle = "Future",
            ContractName = "USD/RUB June 2026 Options",
            ExpirationDate = "2026-06-18",
            ExpirationType = "Standard",
            ExpirationTime = "18:45:00",
            WeekendSession = 0,
        };

    public static CalendarSuspendedReasonDTO MakeValidCalendarSuspensionReasonDto(int id = 1)
        => new()
        {
            Id = id,
            Title = "Corporate action",
        };

    public static CalendarSecurityAttributeDTO MakeValidCalendarSecurityAttributeDto(
        string name = "LOTSIZE")
        => new()
        {
            Name = name,
            Type = "INTEGER",
            Title = "Размер лота",
        };

    // ===== Calendar Group C + market factories =====

    public static CalendarOffDaysMarketDTO MakeValidCalendarMarketOffDayDto()
        => new()
        {
            TradeDate = "2026-05-09",
            IsTraded = 0,
            TradeSessionDate = "2026-05-08",
            Reason = "H",
            UpdateTime = new DateTime(2026, 5, 8, 18, 0, 0, DateTimeKind.Unspecified),
        };

    public static CalendarSessionTypeDTO MakeValidCalendarSessionTypeDto(string type = "MAIN")
        => new()
        {
            Type = type,
            Title = "Основная сессия",
        };
}
