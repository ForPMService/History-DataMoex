namespace History_DataMoex.Parsing
{
    public class ColumnAndNumbersForParsing
    {
        // Фактические номера колонок из columns-map.json

        public readonly record struct ExpectedColumn(int SourceIndex, byte[] Name);

        public static readonly ExpectedColumn[] AlgCandlesExpectedColumns =
        {
            new(0, "open"u8.ToArray()),
            new(1, "close"u8.ToArray()),
            new(2, "high"u8.ToArray()),
            new(3, "low"u8.ToArray()),
            new(4, "value"u8.ToArray()),
            new(5, "volume"u8.ToArray()),
            new(6, "begin"u8.ToArray()),
            new(7, "end"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] AlgCandlesTradeStatExpectedColumns =
        {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "pr_open"u8.ToArray()),
            new(4, "pr_high"u8.ToArray()),
            new(5, "pr_low"u8.ToArray()),
            new(6, "pr_close"u8.ToArray()),
            new(7, "pr_std"u8.ToArray()),
            new(8, "vol"u8.ToArray()),
            new(9, "val"u8.ToArray()),
            new(10, "trades"u8.ToArray()),
            new(11, "pr_vwap"u8.ToArray()),
            new(12, "pr_change"u8.ToArray()),
            new(13, "trades_b"u8.ToArray()),
            new(14, "trades_s"u8.ToArray()),
            new(15, "val_b"u8.ToArray()),
            new(16, "val_s"u8.ToArray()),
            new(17, "vol_b"u8.ToArray()),
            new(18, "vol_s"u8.ToArray()),
            new(19, "disb"u8.ToArray()),
            new(20, "pr_vwap_b"u8.ToArray()),
            new(21, "pr_vwap_s"u8.ToArray()),
            new(22, "SYSTIME"u8.ToArray()),
            new(23, "sec_pr_open"u8.ToArray()),
            new(24, "sec_pr_high"u8.ToArray()),
            new(25, "sec_pr_low"u8.ToArray()),
            new(26, "sec_pr_close"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] FuturesTradeStatsExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "asset_code"u8.ToArray()),
            new(4, "pr_open"u8.ToArray()),
            new(5, "pr_high"u8.ToArray()),
            new(6, "pr_low"u8.ToArray()),
            new(7, "pr_close"u8.ToArray()),
            new(8, "pr_std"u8.ToArray()),
            new(9, "vol"u8.ToArray()),
            new(10, "val"u8.ToArray()),
            new(11, "trades"u8.ToArray()),
            new(12, "pr_vwap"u8.ToArray()),
            new(13, "pr_change"u8.ToArray()),
            new(14, "trades_b"u8.ToArray()),
            new(15, "trades_s"u8.ToArray()),
            new(16, "val_b"u8.ToArray()),
            new(17, "val_s"u8.ToArray()),
            new(18, "vol_b"u8.ToArray()),
            new(19, "vol_s"u8.ToArray()),
            new(20, "disb"u8.ToArray()),
            new(21, "pr_vwap_b"u8.ToArray()),
            new(22, "pr_vwap_s"u8.ToArray()),
            new(23, "im"u8.ToArray()),
            new(24, "oi_open"u8.ToArray()),
            new(25, "oi_high"u8.ToArray()),
            new(26, "oi_low"u8.ToArray()),
            new(27, "oi_close"u8.ToArray()),
            new(28, "sec_pr_open"u8.ToArray()),
            new(29, "sec_pr_high"u8.ToArray()),
            new(30, "sec_pr_low"u8.ToArray()),
            new(31, "sec_pr_close"u8.ToArray()),
            new(32, "SYSTIME"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] AlgOrderBookStats5mExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "spread_bbo"u8.ToArray()),
            new(4, "spread_lv10"u8.ToArray()),
            new(5, "spread_1mio"u8.ToArray()),
            new(6, "levels_b"u8.ToArray()),
            new(7, "levels_s"u8.ToArray()),
            new(8, "vol_b"u8.ToArray()),
            new(9, "vol_s"u8.ToArray()),
            new(10, "val_b"u8.ToArray()),
            new(11, "val_s"u8.ToArray()),
            new(12, "imbalance_vol_bbo"u8.ToArray()),
            new(13, "imbalance_val_bbo"u8.ToArray()),
            new(14, "imbalance_vol"u8.ToArray()),
            new(15, "imbalance_val"u8.ToArray()),
            new(16, "vwap_b"u8.ToArray()),
            new(17, "vwap_s"u8.ToArray()),
            new(18, "vwap_b_1mio"u8.ToArray()),
            new(19, "vwap_s_1mio"u8.ToArray()),
            new(20, "SYSTIME"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] AlgFuturesOrderBookExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "asset_code"u8.ToArray()),
            new(4, "mid_price"u8.ToArray()),
            new(5, "micro_price"u8.ToArray()),
            new(6, "spread_l1"u8.ToArray()),
            new(7, "spread_l2"u8.ToArray()),
            new(8, "spread_l3"u8.ToArray()),
            new(9, "spread_l5"u8.ToArray()),
            new(10, "spread_l10"u8.ToArray()),
            new(11, "spread_l20"u8.ToArray()),
            new(12, "levels_b"u8.ToArray()),
            new(13, "levels_s"u8.ToArray()),
            new(14, "vol_b_l1"u8.ToArray()),
            new(15, "vol_b_l2"u8.ToArray()),
            new(16, "vol_b_l3"u8.ToArray()),
            new(17, "vol_b_l5"u8.ToArray()),
            new(18, "vol_b_l10"u8.ToArray()),
            new(19, "vol_b_l20"u8.ToArray()),
            new(20, "vol_s_l1"u8.ToArray()),
            new(21, "vol_s_l2"u8.ToArray()),
            new(22, "vol_s_l3"u8.ToArray()),
            new(23, "vol_s_l5"u8.ToArray()),
            new(24, "vol_s_l10"u8.ToArray()),
            new(25, "vol_s_l20"u8.ToArray()),
            new(26, "vwap_b_l3"u8.ToArray()),
            new(27, "vwap_b_l5"u8.ToArray()),
            new(28, "vwap_b_l10"u8.ToArray()),
            new(29, "vwap_b_l20"u8.ToArray()),
            new(30, "vwap_s_l3"u8.ToArray()),
            new(31, "vwap_s_l5"u8.ToArray()),
            new(32, "vwap_s_l10"u8.ToArray()),
            new(33, "vwap_s_l20"u8.ToArray()),
            new(34, "SYSTIME"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] AlgOrderStats5mExpectedColumns =
        {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "put_orders_b"u8.ToArray()),
            new(4, "put_orders_s"u8.ToArray()),
            new(5, "put_val_b"u8.ToArray()),
            new(6, "put_val_s"u8.ToArray()),
            new(7, "put_vol_b"u8.ToArray()),
            new(8, "put_vol_s"u8.ToArray()),
            new(9, "put_vwap_b"u8.ToArray()),
            new(10, "put_vwap_s"u8.ToArray()),
            new(11, "put_vol"u8.ToArray()),
            new(12, "put_val"u8.ToArray()),
            new(13, "put_orders"u8.ToArray()),
            new(14, "cancel_orders_b"u8.ToArray()),
            new(15, "cancel_orders_s"u8.ToArray()),
            new(16, "cancel_val_b"u8.ToArray()),
            new(17, "cancel_val_s"u8.ToArray()),
            new(18, "cancel_vol_b"u8.ToArray()),
            new(19, "cancel_vol_s"u8.ToArray()),
            new(20, "cancel_vwap_b"u8.ToArray()),
            new(21, "cancel_vwap_s"u8.ToArray()),
            new(22, "cancel_vol"u8.ToArray()),
            new(23, "cancel_val"u8.ToArray()),
            new(24, "cancel_orders"u8.ToArray()),
            new(25, "SYSTIME"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] Hi2AssetExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "metric"u8.ToArray()),
            new(4, "value"u8.ToArray()),
            new(5, "reference"u8.ToArray()),
            new(6, "SYSTIME"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] Hi2FuturesExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "asset_code"u8.ToArray()),
            new(4, "metric"u8.ToArray()),
            new(5, "value"u8.ToArray()),
            new(6, "reference"u8.ToArray()),
            new(7, "SYSTIME"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] MegaAlertsAssetExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "alert_type"u8.ToArray()),
            new(4, "threshold"u8.ToArray()),
            new(5, "value"u8.ToArray()),
            new(6, "reference"u8.ToArray()),
            new(7, "SYSTIME"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] MegaAlertsFuturesExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradetime"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "asset_code"u8.ToArray()),
            new(4, "alert_type"u8.ToArray()),
            new(5, "threshold"u8.ToArray()),
            new(6, "value"u8.ToArray()),
            new(7, "reference"u8.ToArray()),
            new(8, "SYSTIME"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] FutoiExpectedColumns =
                {
            new(0, "sess_id"u8.ToArray()),
            new(1, "seqnum"u8.ToArray()),
            new(2, "tradedate"u8.ToArray()),
            new(3, "tradetime"u8.ToArray()),
            new(4, "ticker"u8.ToArray()),
            new(5, "clgroup"u8.ToArray()),
            new(6, "pos"u8.ToArray()),
            new(7, "pos_long"u8.ToArray()),
            new(8, "pos_short"u8.ToArray()),
            new(9, "pos_long_num"u8.ToArray()),
            new(10, "pos_short_num"u8.ToArray()),
            new(11, "systime"u8.ToArray()),
            new(12, "trade_session_date"u8.ToArray()),
        };

        // Фактические номера колонок Calendar из columns-map.json

        public static readonly ExpectedColumn[] CalendarOffDaysAllExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "currency_workday"u8.ToArray()),
            new(2, "currency_trade_session_date"u8.ToArray()),
            new(3, "currency_reason"u8.ToArray()),
            new(4, "futures_workday"u8.ToArray()),
            new(5, "futures_trade_session_date"u8.ToArray()),
            new(6, "futures_reason"u8.ToArray()),
            new(7, "stock_workday"u8.ToArray()),
            new(8, "stock_trade_session_date"u8.ToArray()),
            new(9, "stock_reason"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarOffDaysMarketExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "is_traded"u8.ToArray()),
            new(2, "trade_session_date"u8.ToArray()),
            new(3, "reason"u8.ToArray()),
            new(4, "updatetime"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarStockSessionExpectedColumns =
                {
            new(0, "tradedate"u8.ToArray()),
            new(1, "tradingsession"u8.ToArray()),
            new(2, "boardid"u8.ToArray()),
            new(3, "secid"u8.ToArray()),
            new(4, "type"u8.ToArray()),
            new(5, "time_from"u8.ToArray()),
            new(6, "time_till"u8.ToArray()),
            new(7, "updatetime"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarFuturesSessionExpectedColumns =
                {
            new(0, "trade_session_date"u8.ToArray()),
            new(1, "boardid"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "type"u8.ToArray()),
            new(4, "time_from"u8.ToArray()),
            new(5, "time_till"u8.ToArray()),
            new(6, "updatetime"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarSessionTypesExpectedColumns =
                {
            new(0, "type"u8.ToArray()),
            new(1, "title"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarFortsContractsExpectedColumns =
                {
            new(0, "secid"u8.ToArray()),
            new(1, "asset_code"u8.ToArray()),
            new(2, "shortname"u8.ToArray()),
            new(3, "exec_type"u8.ToArray()),
            new(4, "contract_name"u8.ToArray()),
            new(5, "expiration_date"u8.ToArray()),
            new(6, "end_date"u8.ToArray()),
            new(7, "expiration_type"u8.ToArray()),
            new(8, "expiration_time"u8.ToArray()),
            new(9, "weekend_session"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarOptionsSeriesExpectedColumns =
                {
            new(0, "asset_type_name"u8.ToArray()),
            new(1, "asset_code"u8.ToArray()),
            new(2, "series_name"u8.ToArray()),
            new(3, "series_type"u8.ToArray()),
            new(4, "exec_type"u8.ToArray()),
            new(5, "margin_style"u8.ToArray()),
            new(6, "contract_name"u8.ToArray()),
            new(7, "expiration_date"u8.ToArray()),
            new(8, "expiration_type"u8.ToArray()),
            new(9, "expiration_time"u8.ToArray()),
            new(10, "weekend_session"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarSuspendedExpectedColumns =
                {
            new(0, "secid"u8.ToArray()),
            new(1, "reason_id"u8.ToArray()),
            new(2, "date_from"u8.ToArray()),
            new(3, "date_till"u8.ToArray()),
            new(4, "boardid"u8.ToArray()),
            new(5, "settle_codes"u8.ToArray()),
            new(6, "changedate"u8.ToArray()),
            new(7, "updatetime"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarSuspendedReasonsExpectedColumns =
                {
            new(0, "id"u8.ToArray()),
            new(1, "title"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarSecurityChangesExpectedColumns =
                {
            new(0, "updatetime"u8.ToArray()),
            new(1, "action"u8.ToArray()),
            new(2, "secid"u8.ToArray()),
            new(3, "attribute_name"u8.ToArray()),
            new(4, "before_value"u8.ToArray()),
            new(5, "after_value"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarSecurityAttributesExpectedColumns =
                {
            new(0, "name"u8.ToArray()),
            new(1, "type"u8.ToArray()),
            new(2, "title"u8.ToArray()),
        };

        public static readonly ExpectedColumn[] CalendarCursorExpectedColumns =
                {
            new(0, "INDEX"u8.ToArray()),
            new(1, "TOTAL"u8.ToArray()),
            new(2, "PAGESIZE"u8.ToArray()),
        };

        // Фактические номера колонок ISS из columns-map.json.
        // Важно: это НЕ внутренние номера columnIndices[] из ParsingISS.cs.
        // Это реальные позиции колонок в MOEX columns[].

        // ParseIssSecurityStock
        // rootKey: securities
        // source: Securities (stock TQBR)
        // fact columnCount: 27
        // parser uses: 9 columns from 27
        public static readonly ExpectedColumn[] IssStockSecurityExpectedColumns =
                {
            new(0, "SECID"u8.ToArray()),
            new(1, "BOARDID"u8.ToArray()),
            new(2, "SHORTNAME"u8.ToArray()),
            new(4, "LOTSIZE"u8.ToArray()),
            new(5, "FACEVALUE"u8.ToArray()),
            new(9, "SECNAME"u8.ToArray()),
            new(11, "MARKETCODE"u8.ToArray()),
            new(17, "PREVDATE"u8.ToArray()),
            new(22, "PREVLEGALCLOSEPRICE"u8.ToArray()),
        };

        // ParseIssSecurityFutures
        // rootKey: securities
        // source: Securities (futures RFUD)
        // fact columnCount: 26
        // parser uses: 16 columns from 26
        public static readonly ExpectedColumn[] IssFuturesSecurityExpectedColumns =
        {
            new(0, "SECID"u8.ToArray()),
            new(2, "SHORTNAME"u8.ToArray()),
            new(3, "SECNAME"u8.ToArray()),
            new(4, "PREVSETTLEPRICE"u8.ToArray()),
            new(5, "DECIMALS"u8.ToArray()),
            new(6, "MINSTEP"u8.ToArray()),
            new(7, "LASTTRADEDATE"u8.ToArray()),
            new(8, "LASTDELDATE"u8.ToArray()),
            new(11, "ASSETCODE"u8.ToArray()),
            new(12, "PREVOPENPOSITION"u8.ToArray()),
            new(13, "LOTVOLUME"u8.ToArray()),
            new(14, "INITIALMARGIN"u8.ToArray()),
            new(15, "HIGHLIMIT"u8.ToArray()),
            new(16, "LOWLIMIT"u8.ToArray()),
            new(17, "STEPPRICE"u8.ToArray()),
            new(19, "PREVPRICE"u8.ToArray()),
        };
    }
}
