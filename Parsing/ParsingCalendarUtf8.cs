using History_DataMoex.Contracts.Dto;
using History_DataMoex.Contracts.Dto.Calendar;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public static class ParsingCalendarUtf8
    {
        // ═══════════════════════════════════════════════════════════
        // ParseStockSession — два прохода по одним байтам
        // Endpoint: GET /iss/calendars/stock/session.json
        // Таблица 1: session_schedule (8 колонок)
        // Таблица 2: session_schedule.types (2 колонки)
        // ═══════════════════════════════════════════════════════════

        public static (List<CalendarStockSessionDTO> Sessions, List<CalendarSessionTypeDTO> Types)
            ParseStockSession(ReadOnlySpan<byte> jsonBytes)
        {
            // Первый проход — session_schedule
            var sessions = new List<CalendarStockSessionDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarStockSessionSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadStockSessionData(ref reader, sessions, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            // Второй проход — session_schedule.types
            var types = new List<CalendarSessionTypeDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarSessionTypesSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadSessionTypesData(ref reader, types, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            return (sessions, types);
        }

        // ═══════════════════════════════════════════════════════════
        // ParseFuturesSession — два прохода по одним байтам
        // Endpoint: GET /iss/calendars/futures/session.json
        // Таблица 1: session_schedule (7 колонок)
        // Таблица 2: session_schedule.types (2 колонки)
        // ═══════════════════════════════════════════════════════════

        public static (List<CalendarFuturesSessionDTO> Sessions, List<CalendarSessionTypeDTO> Types)
            ParseFuturesSession(ReadOnlySpan<byte> jsonBytes)
        {
            // Первый проход — session_schedule (futures)
            var sessions = new List<CalendarFuturesSessionDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarFuturesSessionSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadFuturesSessionData(ref reader, sessions, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            // Второй проход — session_schedule.types (идентичен stock)
            var types = new List<CalendarSessionTypeDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarSessionTypesSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadSessionTypesData(ref reader, types, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            return (sessions, types);
        }

        // ═══════════════════════════════════════════════════════════
        // ParseFuturesSecurities — два прохода по одним байтам
        // Endpoint: GET /iss/calendars/futures/securities.json
        // Таблица 1: forts (10 колонок)
        // Таблица 2: options (11 колонок)
        // ═══════════════════════════════════════════════════════════

        public static (List<CalendarFortsContractDTO> Forts, List<CalendarOptionsSeriesDTO> Options)
            ParseFuturesSecurities(ReadOnlySpan<byte> jsonBytes)
        {
            // Первый проход — forts
            var forts = new List<CalendarFortsContractDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarFortsContractsSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadFortsData(ref reader, forts, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            // Второй проход — options
            var options = new List<CalendarOptionsSeriesDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarOptionsSeriesSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadOptionsData(ref reader, options, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            return (forts, options);
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк session_schedule (stock) — 8 колонок
        // ═══════════════════════════════════════════════════════════

        private static void ReadStockSessionData(
            ref Utf8JsonReader reader,
            List<CalendarStockSessionDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null;
                int? tradingSession = null;
                string? boardId = null;
                string? secId = null;
                string? type = null;
                string? timeFrom = null;
                string? timeTill = null;
                DateTime? updateTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: tradeDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: tradingSession = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: boardId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: secId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: type = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: timeFrom = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: timeTill = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: updateTime = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarStockSessionDTO
                {
                    TradeDate = tradeDate,
                    TradingSession = tradingSession,
                    BoardId = boardId,
                    SecId = secId,
                    Type = type,
                    TimeFrom = timeFrom,
                    TimeTill = timeTill,
                    UpdateTime = updateTime,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк session_schedule (futures) — 7 колонок
        // ═══════════════════════════════════════════════════════════

        private static void ReadFuturesSessionData(
            ref Utf8JsonReader reader,
            List<CalendarFuturesSessionDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeSessionDate = null;
                string? boardId = null;
                string? secId = null;
                string? type = null;
                DateTime? timeFrom = null;
                DateTime? timeTill = null;
                DateTime? updateTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: tradeSessionDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: boardId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: secId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: type = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: timeFrom = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: timeTill = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: updateTime = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarFuturesSessionDTO
                {
                    TradeSessionDate = tradeSessionDate,
                    BoardId = boardId,
                    SecId = secId,
                    Type = type,
                    TimeFrom = timeFrom,
                    TimeTill = timeTill,
                    UpdateTime = updateTime,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк session_schedule.types — 2 колонки
        // Используется и в ParseStockSession, и в ParseFuturesSession
        // ═══════════════════════════════════════════════════════════

        private static void ReadSessionTypesData(
            ref Utf8JsonReader reader,
            List<CalendarSessionTypeDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? type = null;
                string? title = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: type = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: title = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarSessionTypeDTO
                {
                    Type = type,
                    Title = title,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк forts — 10 колонок
        // ═══════════════════════════════════════════════════════════

        private static void ReadFortsData(
            ref Utf8JsonReader reader,
            List<CalendarFortsContractDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? secId = null;
                string? assetCode = null;
                string? shortName = null;
                string? execType = null;
                string? contractName = null;
                string? expirationDate = null;
                string? endDate = null;
                string? expirationType = null;
                string? expirationTime = null;
                int? weekendSession = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: secId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: assetCode = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: shortName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: execType = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: contractName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: expirationDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: endDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: expirationType = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8: expirationTime = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9: weekendSession = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarFortsContractDTO
                {
                    SecId = secId,
                    AssetCode = assetCode,
                    ShortName = shortName,
                    ExecType = execType,
                    ContractName = contractName,
                    ExpirationDate = expirationDate,
                    EndDate = endDate,
                    ExpirationType = expirationType,
                    ExpirationTime = expirationTime,
                    WeekendSession = weekendSession,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк options — 11 колонок
        // ═══════════════════════════════════════════════════════════

        private static void ReadOptionsData(
            ref Utf8JsonReader reader,
            List<CalendarOptionsSeriesDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? assetTypeName = null;
                string? assetCode = null;
                string? seriesName = null;
                string? seriesType = null;
                string? execType = null;
                string? marginStyle = null;
                string? contractName = null;
                string? expirationDate = null;
                string? expirationType = null;
                string? expirationTime = null;
                int? weekendSession = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: assetTypeName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: assetCode = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: seriesName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: seriesType = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: execType = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: marginStyle = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: contractName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: expirationDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8: expirationType = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9: expirationTime = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 10: weekendSession = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarOptionsSeriesDTO
                {
                    AssetTypeName = assetTypeName,
                    AssetCode = assetCode,
                    SeriesName = seriesName,
                    SeriesType = seriesType,
                    ExecType = execType,
                    MarginStyle = marginStyle,
                    ContractName = contractName,
                    ExpirationDate = expirationDate,
                    ExpirationType = expirationType,
                    ExpirationTime = expirationTime,
                    WeekendSession = weekendSession,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ParseOffDaysAll — один проход, одна таблица
        // Endpoint: GET /iss/calendars.json
        // Таблица: off_days (10 колонок)
        // ═══════════════════════════════════════════════════════════

        public static List<CalendarOffDaysAllDTO> ParseOffDaysAll(ReadOnlySpan<byte> jsonBytes)
        {
            var schema = ColumnAndNumbersForParsing.CalendarOffDaysAllSchema;
            var list = new List<CalendarOffDaysAllDTO>();
            var reader = new Utf8JsonReader(jsonBytes);

            ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

            bool foundColumns = false;
            bool foundData = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                if (reader.ValueTextEquals("columns"u8))
                {
                    foundColumns = true;
                    ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                }
                else if (reader.ValueTextEquals("data"u8))
                {
                    if (!foundColumns)
                        throw new InvalidOperationException(
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                    foundData = true;
                    ReadOffDaysAllData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            return list;
        }

        // ═══════════════════════════════════════════════════════════
        // ParseOffDaysMarket — один проход, одна таблица
        // Endpoint: GET /iss/calendars/stock.json или /iss/calendars/futures.json
        // Таблица: off_days (5 колонок)
        // ═══════════════════════════════════════════════════════════

        public static List<CalendarOffDaysMarketDTO> ParseOffDaysMarket(ReadOnlySpan<byte> jsonBytes)
        {
            var schema = ColumnAndNumbersForParsing.CalendarOffDaysMarketSchema;
            var list = new List<CalendarOffDaysMarketDTO>();
            var reader = new Utf8JsonReader(jsonBytes);

            ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

            bool foundColumns = false;
            bool foundData = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                if (reader.ValueTextEquals("columns"u8))
                {
                    foundColumns = true;
                    ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                }
                else if (reader.ValueTextEquals("data"u8))
                {
                    if (!foundColumns)
                        throw new InvalidOperationException(
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                    foundData = true;
                    ReadOffDaysMarketData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            return list;
        }

        // ═══════════════════════════════════════════════════════════
        // ParseSuspendedWithReasons — три прохода
        // Endpoint: GET /iss/calendars/stock/securities/suspended/details.json
        // Таблица 1: suspended (8 колонок)
        // Таблица 2: suspended.reasons (2 колонки)
        // Таблица 3: suspended.cursor (cursor пагинации)
        // ═══════════════════════════════════════════════════════════

        public static (List<CalendarSuspendedDTO> Suspended, List<CalendarSuspendedReasonDTO> Reasons, PaginationCursorDTO Cursor)
            ParseSuspendedWithReasons(ReadOnlySpan<byte> jsonBytes)
        {
            // Первый проход — suspended
            var suspended = new List<CalendarSuspendedDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarSuspendedSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadSuspendedData(ref reader, suspended, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            // Второй проход — suspended.reasons
            var reasons = new List<CalendarSuspendedReasonDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarSuspendedReasonsSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadSuspendedReasonsData(ref reader, reasons, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            // Третий проход — cursor
            var cursor = ParseHelpersUtf8.ParseCursorUtf8(jsonBytes, "suspended.cursor");

            return (suspended, reasons, cursor);
        }

        // ═══════════════════════════════════════════════════════════
        // ParseSecurityChangesWithAttributes — три прохода
        // Endpoint: GET /iss/calendars/stock/securities/changes.json
        // Таблица 1: securities (6 колонок)
        // Таблица 2: securities.attributes (3 колонки)
        // Таблица 3: securities.cursor (cursor пагинации)
        // ═══════════════════════════════════════════════════════════

        public static (List<CalendarSecurityChangeDTO> Changes, List<CalendarSecurityAttributeDTO> Attributes, PaginationCursorDTO Cursor)
            ParseSecurityChangesWithAttributes(ReadOnlySpan<byte> jsonBytes)
        {
            // Первый проход — securities
            var changes = new List<CalendarSecurityChangeDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarSecurityChangesSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadSecurityChangesData(ref reader, changes, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            // Второй проход — securities.attributes
            var attributes = new List<CalendarSecurityAttributeDTO>();
            {
                var schema = ColumnAndNumbersForParsing.CalendarSecurityAttributesSchema;
                var reader = new Utf8JsonReader(jsonBytes);
                ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

                bool foundColumns = false;
                bool foundData = false;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    if (reader.ValueTextEquals("columns"u8))
                    {
                        foundColumns = true;
                        ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                    }
                    else if (reader.ValueTextEquals("data"u8))
                    {
                        if (!foundColumns)
                            throw new InvalidOperationException(
                                $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                        foundData = true;
                        ReadSecurityAttributesData(ref reader, attributes, schema);
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            }

            // Третий проход — cursor
            var cursor = ParseHelpersUtf8.ParseCursorUtf8(jsonBytes, "securities.cursor");

            return (changes, attributes, cursor);
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк off_days (all) — 10 колонок
        // ═══════════════════════════════════════════════════════════

        private static void ReadOffDaysAllData(
            ref Utf8JsonReader reader,
            List<CalendarOffDaysAllDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null;
                long? currencyWorkday = null;
                string? currencyTradeSessionDate = null;
                string? currencyReason = null;
                long? futuresWorkday = null;
                string? futuresTradeSessionDate = null;
                string? futuresReason = null;
                long? stockWorkday = null;
                string? stockTradeSessionDate = null;
                string? stockReason = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: tradeDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: currencyWorkday = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: currencyTradeSessionDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: currencyReason = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: futuresWorkday = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: futuresTradeSessionDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: futuresReason = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: stockWorkday = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8: stockTradeSessionDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9: stockReason = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarOffDaysAllDTO
                {
                    TradeDate = tradeDate,
                    CurrencyWorkday = currencyWorkday,
                    CurrencyTradeSessionDate = currencyTradeSessionDate,
                    CurrencyReason = currencyReason,
                    FuturesWorkday = futuresWorkday,
                    FuturesTradeSessionDate = futuresTradeSessionDate,
                    FuturesReason = futuresReason,
                    StockWorkday = stockWorkday,
                    StockTradeSessionDate = stockTradeSessionDate,
                    StockReason = stockReason,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк off_days (market) — 5 колонок
        // ═══════════════════════════════════════════════════════════

        private static void ReadOffDaysMarketData(
            ref Utf8JsonReader reader,
            List<CalendarOffDaysMarketDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null;
                int? isTraded = null;
                string? tradeSessionDate = null;
                string? reason = null;
                DateTime? updateTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: tradeDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: isTraded = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: tradeSessionDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: reason = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: updateTime = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarOffDaysMarketDTO
                {
                    TradeDate = tradeDate,
                    IsTraded = isTraded,
                    TradeSessionDate = tradeSessionDate,
                    Reason = reason,
                    UpdateTime = updateTime,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк suspended — 8 колонок
        // ═══════════════════════════════════════════════════════════

        private static void ReadSuspendedData(
            ref Utf8JsonReader reader,
            List<CalendarSuspendedDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? secId = null;
                string? reasonId = null;
                string? dateFrom = null;
                string? dateTill = null;
                string? boardId = null;
                string? settleCodes = null;
                string? changeDate = null;
                DateTime? updateTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: secId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: reasonId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: dateFrom = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: dateTill = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: boardId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: settleCodes = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: changeDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: updateTime = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarSuspendedDTO
                {
                    SecId = secId,
                    ReasonId = reasonId,
                    DateFrom = dateFrom,
                    DateTill = dateTill,
                    BoardId = boardId,
                    SettleCodes = settleCodes,
                    ChangeDate = changeDate,
                    UpdateTime = updateTime,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк suspended.reasons — 2 колонки
        // ═══════════════════════════════════════════════════════════

        private static void ReadSuspendedReasonsData(
            ref Utf8JsonReader reader,
            List<CalendarSuspendedReasonDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                int? id = null;
                string? title = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: id = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: title = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarSuspendedReasonDTO
                {
                    Id = id,
                    Title = title,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк securities (changes) — 6 колонок
        // ═══════════════════════════════════════════════════════════

        private static void ReadSecurityChangesData(
            ref Utf8JsonReader reader,
            List<CalendarSecurityChangeDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                DateTime? updateTime = null;
                string? action = null;
                string? secId = null;
                string? attributeName = null;
                string? beforeValue = null;
                string? afterValue = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: updateTime = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: action = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: secId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: attributeName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: beforeValue = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: afterValue = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarSecurityChangeDTO
                {
                    UpdateTime = updateTime,
                    Action = action,
                    SecId = secId,
                    AttributeName = attributeName,
                    BeforeValue = beforeValue,
                    AfterValue = afterValue,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение строк securities.attributes — 3 колонки
        // ═══════════════════════════════════════════════════════════

        private static void ReadSecurityAttributesData(
            ref Utf8JsonReader reader,
            List<CalendarSecurityAttributeDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? name = null;
                string? type = null;
                string? title = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: name = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: type = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: title = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new CalendarSecurityAttributeDTO
                {
                    Name = name,
                    Type = type,
                    Title = title,
                });

                rowIndex++;
            }
        }
    }
}
