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
}
