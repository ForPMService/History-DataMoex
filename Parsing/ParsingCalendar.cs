using History_DataMoex.DataTransfers.Calendar;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class ParsingCalendar
    {
        public static List<CalendarOffDaysAllDTO> ParseCalendarOffDaysAll(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "off_days");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int tradeDateIndex = GetRequiredIndex(columns, "tradedate", "off_days");
            int currencyWorkdayIndex = GetRequiredIndex(columns, "currency_workday", "off_days");
            int currencyTradeSessionDateIndex = GetRequiredIndex(columns, "currency_trade_session_date", "off_days");
            int currencyReasonIndex = GetRequiredIndex(columns, "currency_reason", "off_days");
            int futuresWorkdayIndex = GetRequiredIndex(columns, "futures_workday", "off_days");
            int futuresTradeSessionDateIndex = GetRequiredIndex(columns, "futures_trade_session_date", "off_days");
            int futuresReasonIndex = GetRequiredIndex(columns, "futures_reason", "off_days");
            int stockWorkdayIndex = GetRequiredIndex(columns, "stock_workday", "off_days");
            int stockTradeSessionDateIndex = GetRequiredIndex(columns, "stock_trade_session_date", "off_days");
            int stockReasonIndex = GetRequiredIndex(columns, "stock_reason", "off_days");

            List<CalendarOffDaysAllDTO> result = new List<CalendarOffDaysAllDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarOffDaysAllDTO dto = new CalendarOffDaysAllDTO
                {
                    TradeDate = ParseHelpers.GetStringOrNull(row[tradeDateIndex]),
                    CurrencyWorkday = ParseHelpers.GetLongOrNull(row[currencyWorkdayIndex]),
                    CurrencyTradeSessionDate = ParseHelpers.GetStringOrNull(row[currencyTradeSessionDateIndex]),
                    CurrencyReason = ParseHelpers.GetStringOrNull(row[currencyReasonIndex]),
                    FuturesWorkday = ParseHelpers.GetLongOrNull(row[futuresWorkdayIndex]),
                    FuturesTradeSessionDate = ParseHelpers.GetStringOrNull(row[futuresTradeSessionDateIndex]),
                    FuturesReason = ParseHelpers.GetStringOrNull(row[futuresReasonIndex]),
                    StockWorkday = ParseHelpers.GetLongOrNull(row[stockWorkdayIndex]),
                    StockTradeSessionDate = ParseHelpers.GetStringOrNull(row[stockTradeSessionDateIndex]),
                    StockReason = ParseHelpers.GetStringOrNull(row[stockReasonIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarOffDaysMarketDTO> ParseCalendarOffDaysMarket(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "off_days");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int tradeDateIndex = GetRequiredIndex(columns, "tradedate", "off_days");
            int isTradedIndex = GetRequiredIndex(columns, "is_traded", "off_days");
            int tradeSessionDateIndex = GetRequiredIndex(columns, "trade_session_date", "off_days");
            int reasonIndex = GetRequiredIndex(columns, "reason", "off_days");
            int updateTimeIndex = GetRequiredIndex(columns, "updatetime", "off_days");

            List<CalendarOffDaysMarketDTO> result = new List<CalendarOffDaysMarketDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarOffDaysMarketDTO dto = new CalendarOffDaysMarketDTO
                {
                    TradeDate = ParseHelpers.GetStringOrNull(row[tradeDateIndex]),
                    IsTraded = ParseHelpers.GetIntOrNull(row[isTradedIndex]),
                    TradeSessionDate = ParseHelpers.GetStringOrNull(row[tradeSessionDateIndex]),
                    Reason = ParseHelpers.GetStringOrNull(row[reasonIndex]),
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(row[updateTimeIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarStockSessionDTO> ParseCalendarStockSession(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "session_schedule");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int tradeDateIndex = GetRequiredIndex(columns, "tradedate", "session_schedule");
            int tradingSessionIndex = GetRequiredIndex(columns, "tradingsession", "session_schedule");
            int boardIdIndex = GetRequiredIndex(columns, "boardid", "session_schedule");
            int secIdIndex = GetRequiredIndex(columns, "secid", "session_schedule");
            int typeIndex = GetRequiredIndex(columns, "type", "session_schedule");
            int timeFromIndex = GetRequiredIndex(columns, "time_from", "session_schedule");
            int timeTillIndex = GetRequiredIndex(columns, "time_till", "session_schedule");
            int updateTimeIndex = GetRequiredIndex(columns, "updatetime", "session_schedule");

            List<CalendarStockSessionDTO> result = new List<CalendarStockSessionDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarStockSessionDTO dto = new CalendarStockSessionDTO
                {
                    TradeDate = ParseHelpers.GetStringOrNull(row[tradeDateIndex]),
                    TradingSession = ParseHelpers.GetIntOrNull(row[tradingSessionIndex]),
                    BoardId = ParseHelpers.GetStringOrNull(row[boardIdIndex]),
                    SecId = ParseHelpers.GetStringOrNull(row[secIdIndex]),
                    Type = ParseHelpers.GetStringOrNull(row[typeIndex]),
                    TimeFrom = ParseHelpers.GetStringOrNull(row[timeFromIndex]),
                    TimeTill = ParseHelpers.GetStringOrNull(row[timeTillIndex]),
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(row[updateTimeIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarFuturesSessionDTO> ParseCalendarFuturesSession(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "session_schedule");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int tradeSessionDateIndex = GetRequiredIndex(columns, "trade_session_date", "session_schedule");
            int boardIdIndex = GetRequiredIndex(columns, "boardid", "session_schedule");
            int secIdIndex = GetRequiredIndex(columns, "secid", "session_schedule");
            int typeIndex = GetRequiredIndex(columns, "type", "session_schedule");
            int timeFromIndex = GetRequiredIndex(columns, "time_from", "session_schedule");
            int timeTillIndex = GetRequiredIndex(columns, "time_till", "session_schedule");
            int updateTimeIndex = GetRequiredIndex(columns, "updatetime", "session_schedule");

            List<CalendarFuturesSessionDTO> result = new List<CalendarFuturesSessionDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarFuturesSessionDTO dto = new CalendarFuturesSessionDTO
                {
                    TradeSessionDate = ParseHelpers.GetStringOrNull(row[tradeSessionDateIndex]),
                    BoardId = ParseHelpers.GetStringOrNull(row[boardIdIndex]),
                    SecId = ParseHelpers.GetStringOrNull(row[secIdIndex]),
                    Type = ParseHelpers.GetStringOrNull(row[typeIndex]),
                    TimeFrom = ParseHelpers.GetDateTimeOrNull(row[timeFromIndex]),
                    TimeTill = ParseHelpers.GetDateTimeOrNull(row[timeTillIndex]),
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(row[updateTimeIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarSessionTypeDTO> ParseCalendarSessionTypes(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "session_schedule.types");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int typeIndex = GetRequiredIndex(columns, "type", "session_schedule.types");
            int titleIndex = GetRequiredIndex(columns, "title", "session_schedule.types");

            List<CalendarSessionTypeDTO> result = new List<CalendarSessionTypeDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarSessionTypeDTO dto = new CalendarSessionTypeDTO
                {
                    Type = ParseHelpers.GetStringOrNull(row[typeIndex]),
                    Title = ParseHelpers.GetStringOrNull(row[titleIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarFortsContractDTO> ParseCalendarFortsContracts(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "forts");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int secIdIndex = GetRequiredIndex(columns, "secid", "forts");
            int assetCodeIndex = GetRequiredIndex(columns, "asset_code", "forts");
            int shortNameIndex = GetRequiredIndex(columns, "shortname", "forts");
            int execTypeIndex = GetRequiredIndex(columns, "exec_type", "forts");
            int contractNameIndex = GetRequiredIndex(columns, "contract_name", "forts");
            int expirationDateIndex = GetRequiredIndex(columns, "expiration_date", "forts");
            int endDateIndex = GetRequiredIndex(columns, "end_date", "forts");
            int expirationTypeIndex = GetRequiredIndex(columns, "expiration_type", "forts");
            int expirationTimeIndex = GetRequiredIndex(columns, "expiration_time", "forts");
            int weekendSessionIndex = GetRequiredIndex(columns, "weekend_session", "forts");

            List<CalendarFortsContractDTO> result = new List<CalendarFortsContractDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarFortsContractDTO dto = new CalendarFortsContractDTO
                {
                    SecId = ParseHelpers.GetStringOrNull(row[secIdIndex]),
                    AssetCode = ParseHelpers.GetStringOrNull(row[assetCodeIndex]),
                    ShortName = ParseHelpers.GetStringOrNull(row[shortNameIndex]),
                    ExecType = ParseHelpers.GetStringOrNull(row[execTypeIndex]),
                    ContractName = ParseHelpers.GetStringOrNull(row[contractNameIndex]),
                    ExpirationDate = ParseHelpers.GetStringOrNull(row[expirationDateIndex]),
                    EndDate = ParseHelpers.GetStringOrNull(row[endDateIndex]),
                    ExpirationType = ParseHelpers.GetStringOrNull(row[expirationTypeIndex]),
                    ExpirationTime = ParseHelpers.GetStringOrNull(row[expirationTimeIndex]),
                    WeekendSession = ParseHelpers.GetIntOrNull(row[weekendSessionIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarOptionsSeriesDTO> ParseCalendarOptionsSeries(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "options");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int assetTypeNameIndex = GetRequiredIndex(columns, "asset_type_name", "options");
            int assetCodeIndex = GetRequiredIndex(columns, "asset_code", "options");
            int seriesNameIndex = GetRequiredIndex(columns, "series_name", "options");
            int seriesTypeIndex = GetRequiredIndex(columns, "series_type", "options");
            int execTypeIndex = GetRequiredIndex(columns, "exec_type", "options");
            int marginStyleIndex = GetRequiredIndex(columns, "margin_style", "options");
            int contractNameIndex = GetRequiredIndex(columns, "contract_name", "options");
            int expirationDateIndex = GetRequiredIndex(columns, "expiration_date", "options");
            int expirationTypeIndex = GetRequiredIndex(columns, "expiration_type", "options");
            int expirationTimeIndex = GetRequiredIndex(columns, "expiration_time", "options");
            int weekendSessionIndex = GetRequiredIndex(columns, "weekend_session", "options");

            List<CalendarOptionsSeriesDTO> result = new List<CalendarOptionsSeriesDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarOptionsSeriesDTO dto = new CalendarOptionsSeriesDTO
                {
                    AssetTypeName = ParseHelpers.GetStringOrNull(row[assetTypeNameIndex]),
                    AssetCode = ParseHelpers.GetStringOrNull(row[assetCodeIndex]),
                    SeriesName = ParseHelpers.GetStringOrNull(row[seriesNameIndex]),
                    SeriesType = ParseHelpers.GetStringOrNull(row[seriesTypeIndex]),
                    ExecType = ParseHelpers.GetStringOrNull(row[execTypeIndex]),
                    MarginStyle = ParseHelpers.GetStringOrNull(row[marginStyleIndex]),
                    ContractName = ParseHelpers.GetStringOrNull(row[contractNameIndex]),
                    ExpirationDate = ParseHelpers.GetStringOrNull(row[expirationDateIndex]),
                    ExpirationType = ParseHelpers.GetStringOrNull(row[expirationTypeIndex]),
                    ExpirationTime = ParseHelpers.GetStringOrNull(row[expirationTimeIndex]),
                    WeekendSession = ParseHelpers.GetIntOrNull(row[weekendSessionIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarSecurityChangeDTO> ParseCalendarSecurityChanges(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "securities");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int updateTimeIndex = GetRequiredIndex(columns, "updatetime", "securities");
            int actionIndex = GetRequiredIndex(columns, "action", "securities");
            int secIdIndex = GetRequiredIndex(columns, "secid", "securities");
            int attributeNameIndex = GetRequiredIndex(columns, "attribute_name", "securities");
            int beforeValueIndex = GetRequiredIndex(columns, "before_value", "securities");
            int afterValueIndex = GetRequiredIndex(columns, "after_value", "securities");

            List<CalendarSecurityChangeDTO> result = new List<CalendarSecurityChangeDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarSecurityChangeDTO dto = new CalendarSecurityChangeDTO
                {
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(row[updateTimeIndex]),
                    Action = ParseHelpers.GetStringOrNull(row[actionIndex]),
                    SecId = ParseHelpers.GetStringOrNull(row[secIdIndex]),
                    AttributeName = ParseHelpers.GetStringOrNull(row[attributeNameIndex]),
                    BeforeValue = ParseHelpers.GetStringOrNull(row[beforeValueIndex]),
                    AfterValue = ParseHelpers.GetStringOrNull(row[afterValueIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarSecurityAttributeDTO> ParseCalendarSecurityAttributes(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "securities.attributes");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int nameIndex = GetRequiredIndex(columns, "name", "securities.attributes");
            int typeIndex = GetRequiredIndex(columns, "type", "securities.attributes");
            int titleIndex = GetRequiredIndex(columns, "title", "securities.attributes");

            List<CalendarSecurityAttributeDTO> result = new List<CalendarSecurityAttributeDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarSecurityAttributeDTO dto = new CalendarSecurityAttributeDTO
                {
                    Name = ParseHelpers.GetStringOrNull(row[nameIndex]),
                    Type = ParseHelpers.GetStringOrNull(row[typeIndex]),
                    Title = ParseHelpers.GetStringOrNull(row[titleIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarSuspendedDTO> ParseCalendarSuspended(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "suspended");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int secIdIndex = GetRequiredIndex(columns, "secid", "suspended");
            int reasonIdIndex = GetRequiredIndex(columns, "reason_id", "suspended");
            int dateFromIndex = GetRequiredIndex(columns, "date_from", "suspended");
            int dateTillIndex = GetRequiredIndex(columns, "date_till", "suspended");
            int boardIdIndex = GetRequiredIndex(columns, "boardid", "suspended");
            int settleCodesIndex = GetRequiredIndex(columns, "settle_codes", "suspended");
            int changeDateIndex = GetRequiredIndex(columns, "changedate", "suspended");
            int updateTimeIndex = GetRequiredIndex(columns, "updatetime", "suspended");

            List<CalendarSuspendedDTO> result = new List<CalendarSuspendedDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarSuspendedDTO dto = new CalendarSuspendedDTO
                {
                    SecId = ParseHelpers.GetStringOrNull(row[secIdIndex]),
                    ReasonId = ParseHelpers.GetStringOrNull(row[reasonIdIndex]),
                    DateFrom = ParseHelpers.GetStringOrNull(row[dateFromIndex]),
                    DateTill = ParseHelpers.GetStringOrNull(row[dateTillIndex]),
                    BoardId = ParseHelpers.GetStringOrNull(row[boardIdIndex]),
                    SettleCodes = ParseHelpers.GetStringOrNull(row[settleCodesIndex]),
                    ChangeDate = ParseHelpers.GetStringOrNull(row[changeDateIndex]),
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(row[updateTimeIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        public static List<CalendarSuspendedReasonDTO> ParseCalendarSuspendedReasons(JsonDocument jsonDocument)
        {
            JsonElement table = GetTable(jsonDocument, "suspended.reasons");
            Dictionary<string, int> columns = GetColumnIndices(table);
            JsonElement data = table.GetProperty("data");

            int idIndex = GetRequiredIndex(columns, "id", "suspended.reasons");
            int titleIndex = GetRequiredIndex(columns, "title", "suspended.reasons");

            List<CalendarSuspendedReasonDTO> result = new List<CalendarSuspendedReasonDTO>(data.GetArrayLength());

            foreach (JsonElement row in data.EnumerateArray())
            {
                CalendarSuspendedReasonDTO dto = new CalendarSuspendedReasonDTO
                {
                    Id = ParseHelpers.GetIntOrNull(row[idIndex]),
                    Title = ParseHelpers.GetStringOrNull(row[titleIndex])
                };

                result.Add(dto);
            }

            return result;
        }

        private static JsonElement GetTable(JsonDocument jsonDocument, string tableName)
        {
            JsonElement root = jsonDocument.RootElement;

            if (!root.TryGetProperty(tableName, out JsonElement table))
            {
                throw new InvalidOperationException(
                    $"MOEX ISS Calendar response does not contain table '{tableName}'.");
            }

            return table;
        }

        private static Dictionary<string, int> GetColumnIndices(JsonElement table)
        {
            JsonElement columns = table.GetProperty("columns");
            Dictionary<string, int> result = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < columns.GetArrayLength(); i++)
            {
                string? columnName = columns[i].GetString();

                if (!string.IsNullOrWhiteSpace(columnName))
                {
                    result[columnName] = i;
                }
            }

            return result;
        }

        private static int GetRequiredIndex(
            Dictionary<string, int> columns,
            string columnName,
            string tableName)
        {
            if (!columns.TryGetValue(columnName, out int index))
            {
                throw new InvalidOperationException(
                    $"MOEX ISS Calendar table '{tableName}' does not contain required column '{columnName}'.");
            }

            return index;
        }
    }
}
