using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.DataTransfers;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class ParsingCalendar
    {
        // ── Off Days (общий, все рынки) ─────────────────────────

        public static List<CalendarOffDaysAllDTO> ParseCalendarOffDaysAll(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "off_days");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 10;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("tradedate"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("currency_workday"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("currency_trade_session_date"u8)) { idx[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("currency_reason"u8)) { idx[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("futures_workday"u8)) { idx[4] = i; found++; continue; }
                else if (columns[i].ValueEquals("futures_trade_session_date"u8)) { idx[5] = i; found++; continue; }
                else if (columns[i].ValueEquals("futures_reason"u8)) { idx[6] = i; found++; continue; }
                else if (columns[i].ValueEquals("stock_workday"u8)) { idx[7] = i; found++; continue; }
                else if (columns[i].ValueEquals("stock_trade_session_date"u8)) { idx[8] = i; found++; continue; }
                else if (columns[i].ValueEquals("stock_reason"u8)) { idx[9] = i; found++; continue; }
            }

            List<CalendarOffDaysAllDTO> result = new List<CalendarOffDaysAllDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarOffDaysAllDTO
                {
                    TradeDate = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    CurrencyWorkday = ParseHelpers.GetLongOrNull(data[i][idx[1]]),
                    CurrencyTradeSessionDate = ParseHelpers.GetStringOrNull(data[i][idx[2]]),
                    CurrencyReason = ParseHelpers.GetStringOrNull(data[i][idx[3]]),
                    FuturesWorkday = ParseHelpers.GetLongOrNull(data[i][idx[4]]),
                    FuturesTradeSessionDate = ParseHelpers.GetStringOrNull(data[i][idx[5]]),
                    FuturesReason = ParseHelpers.GetStringOrNull(data[i][idx[6]]),
                    StockWorkday = ParseHelpers.GetLongOrNull(data[i][idx[7]]),
                    StockTradeSessionDate = ParseHelpers.GetStringOrNull(data[i][idx[8]]),
                    StockReason = ParseHelpers.GetStringOrNull(data[i][idx[9]])
                });
            }

            return result;
        }

        // ── Off Days (один рынок: stock или futures) ────────────

        public static List<CalendarOffDaysMarketDTO> ParseCalendarOffDaysMarket(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "off_days");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 5;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("tradedate"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("is_traded"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("trade_session_date"u8)) { idx[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("reason"u8)) { idx[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("updatetime"u8)) { idx[4] = i; found++; continue; }
            }

            List<CalendarOffDaysMarketDTO> result = new List<CalendarOffDaysMarketDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarOffDaysMarketDTO
                {
                    TradeDate = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    IsTraded = ParseHelpers.GetIntOrNull(data[i][idx[1]]),
                    TradeSessionDate = ParseHelpers.GetStringOrNull(data[i][idx[2]]),
                    Reason = ParseHelpers.GetStringOrNull(data[i][idx[3]]),
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(data[i][idx[4]])
                });
            }

            return result;
        }

        // ── Stock Session ───────────────────────────────────────

        public static List<CalendarStockSessionDTO> ParseCalendarStockSession(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "session_schedule");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 8;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("tradedate"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("tradingsession"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("boardid"u8)) { idx[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("secid"u8)) { idx[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("type"u8)) { idx[4] = i; found++; continue; }
                else if (columns[i].ValueEquals("time_from"u8)) { idx[5] = i; found++; continue; }
                else if (columns[i].ValueEquals("time_till"u8)) { idx[6] = i; found++; continue; }
                else if (columns[i].ValueEquals("updatetime"u8)) { idx[7] = i; found++; continue; }
            }

            List<CalendarStockSessionDTO> result = new List<CalendarStockSessionDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarStockSessionDTO
                {
                    TradeDate = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    TradingSession = ParseHelpers.GetIntOrNull(data[i][idx[1]]),
                    BoardId = ParseHelpers.GetStringOrNull(data[i][idx[2]]),
                    SecId = ParseHelpers.GetStringOrNull(data[i][idx[3]]),
                    Type = ParseHelpers.GetStringOrNull(data[i][idx[4]]),
                    TimeFrom = ParseHelpers.GetStringOrNull(data[i][idx[5]]),
                    TimeTill = ParseHelpers.GetStringOrNull(data[i][idx[6]]),
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(data[i][idx[7]])
                });
            }

            return result;
        }

        // ── Futures Session ─────────────────────────────────────

        public static List<CalendarFuturesSessionDTO> ParseCalendarFuturesSession(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "session_schedule");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 7;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("trade_session_date"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("boardid"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("secid"u8)) { idx[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("type"u8)) { idx[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("time_from"u8)) { idx[4] = i; found++; continue; }
                else if (columns[i].ValueEquals("time_till"u8)) { idx[5] = i; found++; continue; }
                else if (columns[i].ValueEquals("updatetime"u8)) { idx[6] = i; found++; continue; }
            }

            List<CalendarFuturesSessionDTO> result = new List<CalendarFuturesSessionDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarFuturesSessionDTO
                {
                    TradeSessionDate = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    BoardId = ParseHelpers.GetStringOrNull(data[i][idx[1]]),
                    SecId = ParseHelpers.GetStringOrNull(data[i][idx[2]]),
                    Type = ParseHelpers.GetStringOrNull(data[i][idx[3]]),
                    TimeFrom = ParseHelpers.GetDateTimeOrNull(data[i][idx[4]]),
                    TimeTill = ParseHelpers.GetDateTimeOrNull(data[i][idx[5]]),
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(data[i][idx[6]])
                });
            }

            return result;
        }

        // ── Session Types (общий для stock и futures) ───────────

        public static List<CalendarSessionTypeDTO> ParseCalendarSessionTypes(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "session_schedule.types");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 2;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("type"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("title"u8)) { idx[1] = i; found++; continue; }
            }

            List<CalendarSessionTypeDTO> result = new List<CalendarSessionTypeDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarSessionTypeDTO
                {
                    Type = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    Title = ParseHelpers.GetStringOrNull(data[i][idx[1]])
                });
            }

            return result;
        }

        // ── Forts Contracts ─────────────────────────────────────

        public static List<CalendarFortsContractDTO> ParseCalendarFortsContracts(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "forts");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 10;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("secid"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("asset_code"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("shortname"u8)) { idx[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("exec_type"u8)) { idx[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("contract_name"u8)) { idx[4] = i; found++; continue; }
                else if (columns[i].ValueEquals("expiration_date"u8)) { idx[5] = i; found++; continue; }
                else if (columns[i].ValueEquals("end_date"u8)) { idx[6] = i; found++; continue; }
                else if (columns[i].ValueEquals("expiration_type"u8)) { idx[7] = i; found++; continue; }
                else if (columns[i].ValueEquals("expiration_time"u8)) { idx[8] = i; found++; continue; }
                else if (columns[i].ValueEquals("weekend_session"u8)) { idx[9] = i; found++; continue; }
            }

            List<CalendarFortsContractDTO> result = new List<CalendarFortsContractDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarFortsContractDTO
                {
                    SecId = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    AssetCode = ParseHelpers.GetStringOrNull(data[i][idx[1]]),
                    ShortName = ParseHelpers.GetStringOrNull(data[i][idx[2]]),
                    ExecType = ParseHelpers.GetStringOrNull(data[i][idx[3]]),
                    ContractName = ParseHelpers.GetStringOrNull(data[i][idx[4]]),
                    ExpirationDate = ParseHelpers.GetStringOrNull(data[i][idx[5]]),
                    EndDate = ParseHelpers.GetStringOrNull(data[i][idx[6]]),
                    ExpirationType = ParseHelpers.GetStringOrNull(data[i][idx[7]]),
                    ExpirationTime = ParseHelpers.GetStringOrNull(data[i][idx[8]]),
                    WeekendSession = ParseHelpers.GetIntOrNull(data[i][idx[9]])
                });
            }

            return result;
        }

        // ── Options Series ──────────────────────────────────────

        public static List<CalendarOptionsSeriesDTO> ParseCalendarOptionsSeries(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "options");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 11;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("asset_type_name"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("asset_code"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("series_name"u8)) { idx[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("series_type"u8)) { idx[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("exec_type"u8)) { idx[4] = i; found++; continue; }
                else if (columns[i].ValueEquals("margin_style"u8)) { idx[5] = i; found++; continue; }
                else if (columns[i].ValueEquals("contract_name"u8)) { idx[6] = i; found++; continue; }
                else if (columns[i].ValueEquals("expiration_date"u8)) { idx[7] = i; found++; continue; }
                else if (columns[i].ValueEquals("expiration_type"u8)) { idx[8] = i; found++; continue; }
                else if (columns[i].ValueEquals("expiration_time"u8)) { idx[9] = i; found++; continue; }
                else if (columns[i].ValueEquals("weekend_session"u8)) { idx[10] = i; found++; continue; }
            }

            List<CalendarOptionsSeriesDTO> result = new List<CalendarOptionsSeriesDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarOptionsSeriesDTO
                {
                    AssetTypeName = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    AssetCode = ParseHelpers.GetStringOrNull(data[i][idx[1]]),
                    SeriesName = ParseHelpers.GetStringOrNull(data[i][idx[2]]),
                    SeriesType = ParseHelpers.GetStringOrNull(data[i][idx[3]]),
                    ExecType = ParseHelpers.GetStringOrNull(data[i][idx[4]]),
                    MarginStyle = ParseHelpers.GetStringOrNull(data[i][idx[5]]),
                    ContractName = ParseHelpers.GetStringOrNull(data[i][idx[6]]),
                    ExpirationDate = ParseHelpers.GetStringOrNull(data[i][idx[7]]),
                    ExpirationType = ParseHelpers.GetStringOrNull(data[i][idx[8]]),
                    ExpirationTime = ParseHelpers.GetStringOrNull(data[i][idx[9]]),
                    WeekendSession = ParseHelpers.GetIntOrNull(data[i][idx[10]])
                });
            }

            return result;
        }

        // ── Suspended ───────────────────────────────────────────

        public static List<CalendarSuspendedDTO> ParseCalendarSuspended(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "suspended");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 8;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("secid"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("reason_id"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("date_from"u8)) { idx[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("date_till"u8)) { idx[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("boardid"u8)) { idx[4] = i; found++; continue; }
                else if (columns[i].ValueEquals("settle_codes"u8)) { idx[5] = i; found++; continue; }
                else if (columns[i].ValueEquals("changedate"u8)) { idx[6] = i; found++; continue; }
                else if (columns[i].ValueEquals("updatetime"u8)) { idx[7] = i; found++; continue; }
            }

            List<CalendarSuspendedDTO> result = new List<CalendarSuspendedDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarSuspendedDTO
                {
                    SecId = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    ReasonId = ParseHelpers.GetStringOrNull(data[i][idx[1]]),
                    DateFrom = ParseHelpers.GetStringOrNull(data[i][idx[2]]),
                    DateTill = ParseHelpers.GetStringOrNull(data[i][idx[3]]),
                    BoardId = ParseHelpers.GetStringOrNull(data[i][idx[4]]),
                    SettleCodes = ParseHelpers.GetStringOrNull(data[i][idx[5]]),
                    ChangeDate = ParseHelpers.GetStringOrNull(data[i][idx[6]]),
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(data[i][idx[7]])
                });
            }

            return result;
        }

        // ── Suspended Reasons ───────────────────────────────────

        public static List<CalendarSuspendedReasonDTO> ParseCalendarSuspendedReasons(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "suspended.reasons");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 2;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("id"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("title"u8)) { idx[1] = i; found++; continue; }
            }

            List<CalendarSuspendedReasonDTO> result = new List<CalendarSuspendedReasonDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarSuspendedReasonDTO
                {
                    Id = ParseHelpers.GetIntOrNull(data[i][idx[0]]),
                    Title = ParseHelpers.GetStringOrNull(data[i][idx[1]])
                });
            }

            return result;
        }

        // ── Security Changes ────────────────────────────────────

        public static List<CalendarSecurityChangeDTO> ParseCalendarSecurityChanges(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "securities");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 6;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("updatetime"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("action"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("secid"u8)) { idx[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("attribute_name"u8)) { idx[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("before_value"u8)) { idx[4] = i; found++; continue; }
                else if (columns[i].ValueEquals("after_value"u8)) { idx[5] = i; found++; continue; }
            }

            List<CalendarSecurityChangeDTO> result = new List<CalendarSecurityChangeDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarSecurityChangeDTO
                {
                    UpdateTime = ParseHelpers.GetDateTimeOrNull(data[i][idx[0]]),
                    Action = ParseHelpers.GetStringOrNull(data[i][idx[1]]),
                    SecId = ParseHelpers.GetStringOrNull(data[i][idx[2]]),
                    AttributeName = ParseHelpers.GetStringOrNull(data[i][idx[3]]),
                    BeforeValue = ParseHelpers.GetStringOrNull(data[i][idx[4]]),
                    AfterValue = ParseHelpers.GetStringOrNull(data[i][idx[5]])
                });
            }

            return result;
        }

        // ── Security Attributes ─────────────────────────────────

        public static List<CalendarSecurityAttributeDTO> ParseCalendarSecurityAttributes(JsonDocument jsonDocument)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, "securities.attributes");
            JsonElement columns = table.GetProperty("columns");
            JsonElement data = table.GetProperty("data");

            const int arraysize = 3;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("name"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("type"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("title"u8)) { idx[2] = i; found++; continue; }
            }

            List<CalendarSecurityAttributeDTO> result = new List<CalendarSecurityAttributeDTO>(data.GetArrayLength());

            for (int i = 0; i < data.GetArrayLength(); i++)
            {
                result.Add(new CalendarSecurityAttributeDTO
                {
                    Name = ParseHelpers.GetStringOrNull(data[i][idx[0]]),
                    Type = ParseHelpers.GetStringOrNull(data[i][idx[1]]),
                    Title = ParseHelpers.GetStringOrNull(data[i][idx[2]])
                });
            }

            return result;
        }

        // ── Cursor (универсальный для Calendar) ─────────────────

        /// <summary>
        /// Парсинг cursor-пагинации для Calendar endpoint'ов.
        ///
        /// cursorKey — имя JSON-таблицы с курсором.
        /// Примеры: "suspended.cursor", "securities.cursor".
        ///
        /// Структура всегда одинаковая: INDEX, TOTAL, PAGESIZE.
        /// </summary>
        public static PaginationCursorDTO ParseCursor(JsonDocument jsonDocument, string cursorKey)
        {
            JsonElement root = jsonDocument.RootElement;
            JsonElement table = GetTable(root, cursorKey);
            JsonElement columns = table.GetProperty("columns");

            const int arraysize = 3;
            Span<int> idx = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("INDEX"u8)) { idx[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("TOTAL"u8)) { idx[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("PAGESIZE"u8)) { idx[2] = i; found++; continue; }
            }

            JsonElement datas = table.GetProperty("data");

            return new PaginationCursorDTO()
            {
                Index = ParseHelpers.GetIntOrNull(datas[0][idx[0]]),
                Total = ParseHelpers.GetIntOrNull(datas[0][idx[1]]),
                PageSize = ParseHelpers.GetIntOrNull(datas[0][idx[2]])
            };
        }

        // ── Инфраструктура ──────────────────────────────────────

        private static JsonElement GetTable(JsonElement root, string tableName)
        {
            if (!root.TryGetProperty(tableName, out JsonElement table))
            {
                throw new InvalidOperationException(
                    $"MOEX ISS Calendar response does not contain table '{tableName}'.");
            }

            return table;
        }
    }
}
