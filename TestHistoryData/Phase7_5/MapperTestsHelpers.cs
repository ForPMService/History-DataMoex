using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Mappers;

namespace TestHistoryData.Phase7_5;

/// <summary>
/// Общие фабрики DTO + MapContext для всех тест-классов phase 7.5-C.
/// Make*Dto заполняет осмысленные значения для всех hash-полей маппера —
/// нужно для Map_HashStability (RowHashV1 не должен быть 0).
/// MakeMinimal*Dto заполняет только TradeDate/TradeTime/SecId — для Map_NullFields_HashStable.
/// </summary>
internal static class MapperTestsHelpers
{
    public static MapContext MakeContext(string sourceCode, string sourceTimezone)
        => new(
            SourceCode: sourceCode,
            Endpoint: "test",
            SourceTimezone: sourceTimezone,
            LoadJobId: Guid.CreateVersion7(),
            RawObjectId: Guid.CreateVersion7(),
            FetchedAtUtc: new DateTime(2026, 5, 7, 12, 0, 0, DateTimeKind.Utc));

    // ── TradeStats ────────────────────────────────────────────────────────

    public static SuperCandlesTradeStats5mDTO MakeValidTradeStatsDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        PrOpen = 300.5,
        PrHigh = 301.0,
        PrLow = 300.0,
        PrClose = 300.7,
        PrStd = 0.3,
        Vol = 1000,
        Val = 300500.0,
        Trades = 25,
        PrVwap = 300.6,
        PrChange = 0.2,
        TradesB = 12,
        TradesS = 13,
        ValB = 150000.0,
        ValS = 150500.0,
        VolB = 600,
        VolS = 400,
        Disb = 0.1,
        PrVwapB = 300.55,
        PrVwapS = 300.65,
        SecPrOpen = 10,
        SecPrHigh = 20,
        SecPrLow = 5,
        SecPrClose = 290,
    };

    public static SuperCandlesTradeStats5mDTO MakeMinimalTradeStatsDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };

    // ── FuturesTradeStats ─────────────────────────────────────────────────

    public static SuperCandlesFuturesTradeStats5mDTO MakeValidFuturesTradeStatsDto(
        string? tradeDate, string? tradeTime, string? secId, string? assetCode = "Si") => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        AssetCode = assetCode,
        PrOpen = 80000.0,
        PrHigh = 80100.0,
        PrLow = 79900.0,
        PrClose = 80050.0,
        PrStd = 50.0,
        Vol = 250,
        Val = 20012500,
        Trades = 15,
        PrVwap = 80025.0,
        PrChange = 50.0,
        TradesB = 8,
        TradesS = 7,
        ValB = 10000000.0,
        ValS = 10012500.0,
        VolB = 125,
        VolS = 125,
        Disb = 0.02,
        PrVwapB = 80020.0,
        PrVwapS = 80030.0,
        Im = 5000.0,
        OiOpen = 900,
        OiHigh = 1100,
        OiLow = 850,
        OiClose = 1000,
        SecPrOpen = 1,
        SecPrHigh = 60,
        SecPrLow = 30,
        SecPrClose = 299,
    };

    public static SuperCandlesFuturesTradeStats5mDTO MakeMinimalFuturesTradeStatsDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };

    // ── OrderBookStats ────────────────────────────────────────────────────

    public static SuperCandlesOrderBookStats5mDTO MakeValidOrderBookStatsDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        SpreadBbo = 0.05,
        SpreadLv10 = 0.5,
        Spread1Mio = 1.2,
        LevelsB = 10,
        LevelsS = 10,
        VolB = 1000,
        VolS = 800,
        ValB = 300500,
        ValS = 240400,
        ImbalanceVolBbo = 0.1,
        ImbalanceValBbo = 0.12,
        ImbalanceVol = 0.05,
        ImbalanceVal = 0.06,
        VwapB = 300.5,
        VwapS = 300.7,
        VwapB1Mio = 300.45,
        VwapS1Mio = 300.75,
    };

    public static SuperCandlesOrderBookStats5mDTO MakeMinimalOrderBookStatsDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };

    // ── FuturesOrderBookStats ─────────────────────────────────────────────

    public static SuperCandlesFuturesOrderBookStats5mDTO MakeValidFuturesOrderBookStatsDto(
        string? tradeDate, string? tradeTime, string? secId, string? assetCode = "Si") => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        AssetCode = assetCode,
        MidPrice = 80025.0,
        MicroPrice = 80024.5,
        SpreadL1 = 0.5,
        SpreadL2 = 1.0,
        SpreadL3 = 1.5,
        SpreadL5 = 2.5,
        SpreadL10 = 5.0,
        SpreadL20 = 10.0,
        LevelsB = 20,
        LevelsS = 20,
        VolBL1 = 100,
        VolBL2 = 200,
        VolBL3 = 300,
        VolBL5 = 500,
        VolBL10 = 1000,
        VolBL20 = 2000,
        VolSL1 = 90,
        VolSL2 = 180,
        VolSL3 = 270,
        VolSL5 = 450,
        VolSL10 = 900,
        VolSL20 = 1800,
        VwapBL3 = 80023.0,
        VwapBL5 = 80022.5,
        VwapBL10 = 80021.0,
        VwapBL20 = 80018.0,
        VwapSL3 = 80027.0,
        VwapSL5 = 80027.5,
        VwapSL10 = 80029.0,
        VwapSL20 = 80032.0,
    };

    public static SuperCandlesFuturesOrderBookStats5mDTO MakeMinimalFuturesOrderBookStatsDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };

    // ── OrderStats ────────────────────────────────────────────────────────

    public static SuperCandlesOrderStats5mDTO MakeValidOrderStatsDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        PutOrdersB = 25,
        PutOrdersS = 30,
        PutValB = 250000.0,
        PutValS = 270000.0,
        PutVolB = 800,
        PutVolS = 900,
        PutVwapB = 312.5,
        PutVwapS = 300.0,
        PutVol = 1700,
        PutVal = 520000.0,
        PutOrders = 55,
        CancelOrdersB = 5,
        CancelOrdersS = 6,
        CancelValB = 50000.0,
        CancelValS = 60000.0,
        CancelVolB = 200,
        CancelVolS = 240,
        CancelVwapB = 300.0,
        CancelVwapS = 300.5,
        CancelVol = 440,
        CancelVal = 110000.0,
        CancelOrders = 11,
    };

    public static SuperCandlesOrderStats5mDTO MakeMinimalOrderStatsDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };

    // ── FUTOI ─────────────────────────────────────────────────────────────

    public static FutoiDTO MakeValidFutoiDto(
        string? tradeDate, string? tradeTime, string? ticker, string? clGroup = "FIZ") => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        Ticker = ticker,
        ClGroup = clGroup,
        SessId = 1,
        SeqNum = 42,
        Pos = 1500,
        PosLong = 1000,
        PosShort = -500,
        PosLongNum = 50,
        PosShortNum = 30,
        TradeSessionDate = "2026-05-07",
    };

    public static FutoiDTO MakeMinimalFutoiDto(
        string? tradeDate, string? tradeTime, string? ticker) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        Ticker = ticker,
    };

    // ── Hi2Stock ──────────────────────────────────────────────────────────

    public static Hi2AssetDTO MakeValidHi2StockDto(
        string? tradeDate, string? tradeTime, string? secId, string? metric = "hhi_volume") => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        Metric = metric,
        Value = 0.42,
        Reference = "",
    };

    public static Hi2AssetDTO MakeMinimalHi2StockDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };

    // ── Hi2Futures ────────────────────────────────────────────────────────

    public static Hi2FuturesDTO MakeValidHi2FuturesDto(
        string? tradeDate, string? tradeTime, string? secId,
        string? assetCode = "Si", string? metric = "hhi_volume") => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        AssetCode = assetCode,
        Metric = metric,
        Value = 0.33,
        Reference = "",
    };

    public static Hi2FuturesDTO MakeMinimalHi2FuturesDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };

    // ── MegaAlertsStock ───────────────────────────────────────────────────

    public static MegaAlertsAssetsDTO MakeValidMegaAlertsStockDto(
        string? tradeDate, string? tradeTime, string? secId,
        string? alertType = "vol_99_9_pctl") => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        AlertType = alertType,
        Threshold = 100000.0,
        Value = 250000.0,
        Reference = "{}",
    };

    public static MegaAlertsAssetsDTO MakeMinimalMegaAlertsStockDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };

    // ── MegaAlertsFutures ─────────────────────────────────────────────────

    public static MegaAlertsFuturesDTO MakeValidMegaAlertsFuturesDto(
        string? tradeDate, string? tradeTime, string? secId,
        string? assetCode = "Si",
        string? alertType = "oi_close_change_99_9_pctl-") => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
        AssetCode = assetCode,
        AlertType = alertType,
        Threshold = 100.0,
        Value = 150.0,
        Reference = "{}",
    };

    public static MegaAlertsFuturesDTO MakeMinimalMegaAlertsFuturesDto(
        string? tradeDate, string? tradeTime, string? secId) => new()
    {
        TradeDate = tradeDate,
        TradeTime = tradeTime,
        SecId = secId,
    };
}
