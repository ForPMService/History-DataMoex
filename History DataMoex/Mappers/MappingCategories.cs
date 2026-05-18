namespace History_DataMoex.Mappers;

/// <summary>
/// Стабильные строки-категории mapping-слоя. public — чтобы тесты использовали без InternalsVisibleTo.
/// </summary>
public static class MappingCategories
{
    public const string Candles = "candles";
    public const string TradeStats = "tradestats";
    public const string OrderBookStats = "obstats";
    public const string OrderStats = "orderstats";
    public const string FuturesTradeStats = "futures_tradestats";
    public const string FuturesOrderBookStats = "futures_obstats";
    public const string Futoi = "futoi";
    public const string Hi2Stock = "hi2_stock";
    public const string Hi2Futures = "hi2_futures";
    public const string MegaAlertsStock = "megaalerts_stock";
    public const string MegaAlertsFutures = "megaalerts_futures";

    // ----- Phase 8 — ISS -----
    public const string IssStockSecurity = "iss_stock_security";
    public const string IssFuturesSecurity = "iss_futures_security";

    // ----- Phase 8 — Calendar (11 категорий по 3 группам §9 Lock) -----
    // Group A — SecIdRequired
    public const string CalendarFortsContract = "calendar_forts_contract";
    public const string CalendarSuspension = "calendar_suspension";
    public const string CalendarSecurityChange = "calendar_security_change";

    // Group B — SecId nullable
    public const string CalendarStockSession = "calendar_stock_session";
    public const string CalendarFuturesSession = "calendar_futures_session";

    // Group C — SecId not applicable
    public const string CalendarOffDayAll = "calendar_offday_all";
    public const string CalendarOptionsSeries = "calendar_options_series";
    public const string CalendarSuspensionReason = "calendar_suspension_reason";
    public const string CalendarSecurityAttribute = "calendar_security_attribute";

    // Group C + market параметр
    public const string CalendarMarketOffDay = "calendar_market_offday";
    public const string CalendarSessionType = "calendar_session_type";
}
