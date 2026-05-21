using History_DataMoex.Contracts.Dto;
using History_DataMoex.Contracts.Dto.Algopack;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class ParsingAlgUtf8
    {
        public static List<CandlesDTO> ParseAlgCandles(ReadOnlySpan<byte> jsonBytes)
        {
            var schema = ColumnAndNumbersForParsing.AlgCandlesSchema;
            var candlesList = new List<CandlesDTO>();
            var reader = new Utf8JsonReader(jsonBytes);

            // ── Шаг 1. Найти RootKey на верхнем уровне JSON (A1) ──
            ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

            // ── Шаг 2. Читать свойства ТОЛЬКО внутри RootKey-объекта ──
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
                    // A2: data без предшествующего columns — ошибка.
                    // Без валидации схемы данные нельзя читать —
                    // поля могут оказаться не в тех позициях.
                    if (!foundColumns)
                    {
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");
                    }

                    foundData = true;
                    ReadCandlesData(ref reader, candlesList, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            // ── Шаг 3. Проверить что нашли обязательные секции ──
            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            return candlesList;
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение данных свечей (A3 + A5)
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Чтение массива строк данных свечей через ReadDataRow.
        /// 
        /// ReadDataRow (A3) итерируется по TotalColumns позициям,
        /// вызывает делегат только для нужных колонок,
        /// остальные пропускает.
        /// 
        /// Для свечей TotalColumns == Columns.Length == 8,
        /// пропусков нет, но паттерн единый для всех парсеров.
        /// </summary>
        private static void ReadCandlesData(
            ref Utf8JsonReader reader,
            List<CandlesDTO> candlesList,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                // reader стоит на StartArray внутреннего массива (одна строка)
                double? open = null, close = null, high = null, low = null;
                double? value = null, volume = null;
                DateTime? begin = null, end = null;

                // A3: ReadDataRow проходит по TotalColumns позициям,
                // вызывает делегат только для позиций из schema.Columns.
                // A4: если строка короче TotalColumns — MoexSchemaMismatchException.
                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: open = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: close = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: high = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: low = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: value = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: volume = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            // A5: ReadDateTimeUtf8 без GetString()
                            case 6: begin = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: end = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                candlesList.Add(new CandlesDTO
                {
                    Open = open,
                    Close = close,
                    High = high,
                    Low = low,
                    Value = value,
                    Volume = volume,
                    Begin = begin,
                    End = end
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // TradeStats Stock (акции) — 27 колонок (B2)
        // ═══════════════════════════════════════════════════════════

        public static List<SuperCandlesTradeStats5mDTO> ParseTradeStatsStock(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseTradeStatsStock(jsonBytes, out _);
        }

        public static List<SuperCandlesTradeStats5mDTO> ParseTradeStatsStock(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.AlgCandlesTradeStatSchema;
            var list = new List<SuperCandlesTradeStats5mDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                    foundData = true;
                    ReadTradeStatsStockData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadTradeStatsStockData(
            ref Utf8JsonReader reader,
            List<SuperCandlesTradeStats5mDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null;
                double? prOpen = null, prHigh = null, prLow = null, prClose = null, prStd = null;
                int? vol = null;
                double? val = null;
                int? trades = null;
                double? prVwap = null, prChange = null;
                int? tradesB = null, tradesS = null;
                double? valB = null, valS = null;
                long? volB = null, volS = null;
                double? disb = null, prVwapB = null, prVwapS = null;
                DateTime? sysTime = null;
                int? secPrOpen = null, secPrHigh = null, secPrLow = null, secPrClose = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0:  tradeDate  = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1:  tradeTime  = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2:  secId      = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3:  prOpen     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4:  prHigh     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5:  prLow      = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6:  prClose    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7:  prStd      = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8:  vol        = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9:  val        = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 10: trades     = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 11: prVwap     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 12: prChange   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 13: tradesB    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 14: tradesS    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 15: valB       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 16: valS       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 17: volB       = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 18: volS       = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 19: disb       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 20: prVwapB    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 21: prVwapS    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 22: sysTime    = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                            case 23: secPrOpen  = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 24: secPrHigh  = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 25: secPrLow   = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 26: secPrClose = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new SuperCandlesTradeStats5mDTO
                {
                    TradeDate  = tradeDate,
                    TradeTime  = tradeTime,
                    SecId      = secId,
                    PrOpen     = prOpen,
                    PrHigh     = prHigh,
                    PrLow      = prLow,
                    PrClose    = prClose,
                    PrStd      = prStd,
                    Vol        = vol,
                    Val        = val,
                    Trades     = trades,
                    PrVwap     = prVwap,
                    PrChange   = prChange,
                    TradesB    = tradesB,
                    TradesS    = tradesS,
                    ValB       = valB,
                    ValS       = valS,
                    VolB       = volB,
                    VolS       = volS,
                    Disb       = disb,
                    PrVwapB    = prVwapB,
                    PrVwapS    = prVwapS,
                    SysTime    = sysTime,
                    SecPrOpen  = secPrOpen,
                    SecPrHigh  = secPrHigh,
                    SecPrLow   = secPrLow,
                    SecPrClose = secPrClose,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // TradeStats Futures (фьючерсы) — 33 колонки (B2)
        // ═══════════════════════════════════════════════════════════

        public static List<SuperCandlesFuturesTradeStats5mDTO> ParseTradeStatsFutures(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseTradeStatsFutures(jsonBytes, out _);
        }

        public static List<SuperCandlesFuturesTradeStats5mDTO> ParseTradeStatsFutures(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.FuturesTradeStatsSchema;
            var list = new List<SuperCandlesFuturesTradeStats5mDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                    foundData = true;
                    ReadTradeStatsFuturesData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadTradeStatsFuturesData(
            ref Utf8JsonReader reader,
            List<SuperCandlesFuturesTradeStats5mDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null, assetCode = null;
                double? prOpen = null, prHigh = null, prLow = null, prClose = null, prStd = null;
                long? vol = null, val = null;
                int? trades = null;
                double? prVwap = null, prChange = null;
                int? tradesB = null, tradesS = null;
                double? valB = null, valS = null;
                long? volB = null, volS = null;
                double? disb = null, prVwapB = null, prVwapS = null, im = null;
                long? oiOpen = null, oiHigh = null, oiLow = null, oiClose = null;
                int? secPrOpen = null, secPrHigh = null, secPrLow = null, secPrClose = null;
                DateTime? sysTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0:  tradeDate  = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1:  tradeTime  = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2:  secId      = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3:  assetCode  = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4:  prOpen     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5:  prHigh     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6:  prLow      = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7:  prClose    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8:  prStd      = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9:  vol        = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 10: val        = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 11: trades     = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 12: prVwap     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 13: prChange   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 14: tradesB    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 15: tradesS    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 16: valB       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 17: valS       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 18: volB       = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 19: volS       = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 20: disb       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 21: prVwapB    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 22: prVwapS    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 23: im         = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 24: oiOpen     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 25: oiHigh     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 26: oiLow      = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 27: oiClose    = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 28: secPrOpen  = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 29: secPrHigh  = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 30: secPrLow   = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 31: secPrClose = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 32: sysTime    = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new SuperCandlesFuturesTradeStats5mDTO
                {
                    TradeDate  = tradeDate,
                    TradeTime  = tradeTime,
                    SecId      = secId,
                    AssetCode  = assetCode,
                    PrOpen     = prOpen,
                    PrHigh     = prHigh,
                    PrLow      = prLow,
                    PrClose    = prClose,
                    PrStd      = prStd,
                    Vol        = vol,
                    Val        = val,
                    Trades     = trades,
                    PrVwap     = prVwap,
                    PrChange   = prChange,
                    TradesB    = tradesB,
                    TradesS    = tradesS,
                    ValB       = valB,
                    ValS       = valS,
                    VolB       = volB,
                    VolS       = volS,
                    Disb       = disb,
                    PrVwapB    = prVwapB,
                    PrVwapS    = prVwapS,
                    Im         = im,
                    OiOpen     = oiOpen,
                    OiHigh     = oiHigh,
                    OiLow      = oiLow,
                    OiClose    = oiClose,
                    SecPrOpen  = secPrOpen,
                    SecPrHigh  = secPrHigh,
                    SecPrLow   = secPrLow,
                    SecPrClose = secPrClose,
                    SysTime    = sysTime,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // OBStats Stock (акции) — 21 колонка (B3)
        // ═══════════════════════════════════════════════════════════

        public static List<SuperCandlesOrderBookStats5mDTO> ParseOBStatsStock(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseOBStatsStock(jsonBytes, out _);
        }

        public static List<SuperCandlesOrderBookStats5mDTO> ParseOBStatsStock(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.AlgOrderBookStats5mSchema;
            var list = new List<SuperCandlesOrderBookStats5mDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                    foundData = true;
                    ReadOBStatsStockData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadOBStatsStockData(
            ref Utf8JsonReader reader,
            List<SuperCandlesOrderBookStats5mDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null;
                double? spreadBbo = null, spreadLv10 = null, spread1Mio = null;
                int? levelsB = null, levelsS = null;
                long? volB = null, volS = null, valB = null, valS = null;
                double? imbalanceVolBbo = null, imbalanceValBbo = null;
                double? imbalanceVol = null, imbalanceVal = null;
                double? vwapB = null, vwapS = null, vwapB1Mio = null, vwapS1Mio = null;
                DateTime? sysTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0:  tradeDate        = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1:  tradeTime        = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2:  secId            = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3:  spreadBbo        = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4:  spreadLv10       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5:  spread1Mio       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6:  levelsB          = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7:  levelsS          = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8:  volB             = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9:  volS             = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 10: valB             = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 11: valS             = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 12: imbalanceVolBbo  = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 13: imbalanceValBbo  = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 14: imbalanceVol     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 15: imbalanceVal     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 16: vwapB            = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 17: vwapS            = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 18: vwapB1Mio        = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 19: vwapS1Mio        = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 20: sysTime          = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new SuperCandlesOrderBookStats5mDTO
                {
                    TradeDate       = tradeDate,
                    TradeTime       = tradeTime,
                    SecId           = secId,
                    SpreadBbo       = spreadBbo,
                    SpreadLv10      = spreadLv10,
                    Spread1Mio      = spread1Mio,
                    LevelsB         = levelsB,
                    LevelsS         = levelsS,
                    VolB            = volB,
                    VolS            = volS,
                    ValB            = valB,
                    ValS            = valS,
                    ImbalanceVolBbo = imbalanceVolBbo,
                    ImbalanceValBbo = imbalanceValBbo,
                    ImbalanceVol    = imbalanceVol,
                    ImbalanceVal    = imbalanceVal,
                    VwapB           = vwapB,
                    VwapS           = vwapS,
                    VwapB1Mio       = vwapB1Mio,
                    VwapS1Mio       = vwapS1Mio,
                    SysTime         = sysTime,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // OBStats Futures (фьючерсы) — 35 колонок (B3)
        // ═══════════════════════════════════════════════════════════

        public static List<SuperCandlesFuturesOrderBookStats5mDTO> ParseOBStatsFutures(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseOBStatsFutures(jsonBytes, out _);
        }

        public static List<SuperCandlesFuturesOrderBookStats5mDTO> ParseOBStatsFutures(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.AlgFuturesOrderBookSchema;
            var list = new List<SuperCandlesFuturesOrderBookStats5mDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                    foundData = true;
                    ReadOBStatsFuturesData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadOBStatsFuturesData(
            ref Utf8JsonReader reader,
            List<SuperCandlesFuturesOrderBookStats5mDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null, assetCode = null;
                double? midPrice = null, microPrice = null;
                double? spreadL1 = null, spreadL2 = null, spreadL3 = null, spreadL5 = null;
                double? spreadL10 = null, spreadL20 = null;
                int? levelsB = null, levelsS = null;
                long? volBL1 = null, volBL2 = null, volBL3 = null, volBL5 = null;
                long? volBL10 = null, volBL20 = null;
                long? volSL1 = null, volSL2 = null, volSL3 = null, volSL5 = null;
                long? volSL10 = null, volSL20 = null;
                double? vwapBL3 = null, vwapBL5 = null, vwapBL10 = null, vwapBL20 = null;
                double? vwapSL3 = null, vwapSL5 = null, vwapSL10 = null, vwapSL20 = null;
                DateTime? sysTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0:  tradeDate  = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1:  tradeTime  = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2:  secId      = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3:  assetCode  = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4:  midPrice   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5:  microPrice = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6:  spreadL1   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7:  spreadL2   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8:  spreadL3   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9:  spreadL5   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 10: spreadL10  = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 11: spreadL20  = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 12: levelsB    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 13: levelsS    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 14: volBL1     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 15: volBL2     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 16: volBL3     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 17: volBL5     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 18: volBL10    = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 19: volBL20    = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 20: volSL1     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 21: volSL2     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 22: volSL3     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 23: volSL5     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 24: volSL10    = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 25: volSL20    = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 26: vwapBL3    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 27: vwapBL5    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 28: vwapBL10   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 29: vwapBL20   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 30: vwapSL3    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 31: vwapSL5    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 32: vwapSL10   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 33: vwapSL20   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 34: sysTime    = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new SuperCandlesFuturesOrderBookStats5mDTO
                {
                    TradeDate  = tradeDate,
                    TradeTime  = tradeTime,
                    SecId      = secId,
                    AssetCode  = assetCode,
                    MidPrice   = midPrice,
                    MicroPrice = microPrice,
                    SpreadL1   = spreadL1,
                    SpreadL2   = spreadL2,
                    SpreadL3   = spreadL3,
                    SpreadL5   = spreadL5,
                    SpreadL10  = spreadL10,
                    SpreadL20  = spreadL20,
                    LevelsB    = levelsB,
                    LevelsS    = levelsS,
                    VolBL1     = volBL1,
                    VolBL2     = volBL2,
                    VolBL3     = volBL3,
                    VolBL5     = volBL5,
                    VolBL10    = volBL10,
                    VolBL20    = volBL20,
                    VolSL1     = volSL1,
                    VolSL2     = volSL2,
                    VolSL3     = volSL3,
                    VolSL5     = volSL5,
                    VolSL10    = volSL10,
                    VolSL20    = volSL20,
                    VwapBL3    = vwapBL3,
                    VwapBL5    = vwapBL5,
                    VwapBL10   = vwapBL10,
                    VwapBL20   = vwapBL20,
                    VwapSL3    = vwapSL3,
                    VwapSL5    = vwapSL5,
                    VwapSL10   = vwapSL10,
                    VwapSL20   = vwapSL20,
                    SysTime    = sysTime,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // OrderStats Stock (акции) — 26 колонок (B4)
        // ═══════════════════════════════════════════════════════════

        public static List<SuperCandlesOrderStats5mDTO> ParseOrderStatsStock(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseOrderStatsStock(jsonBytes, out _);
        }

        public static List<SuperCandlesOrderStats5mDTO> ParseOrderStatsStock(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.AlgOrderStats5mSchema;
            var list = new List<SuperCandlesOrderStats5mDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");

                    foundData = true;
                    ReadOrderStatsStockData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadOrderStatsStockData(
            ref Utf8JsonReader reader,
            List<SuperCandlesOrderStats5mDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null;
                int? putOrdersB = null, putOrdersS = null;
                double? putValB = null, putValS = null;
                int? putVolB = null, putVolS = null;
                double? putVwapB = null, putVwapS = null;
                int? putVol = null;
                double? putVal = null;
                int? putOrders = null;
                int? cancelOrdersB = null, cancelOrdersS = null;
                double? cancelValB = null, cancelValS = null;
                int? cancelVolB = null;
                long? cancelVolS = null;
                double? cancelVwapB = null, cancelVwapS = null;
                long? cancelVol = null;
                double? cancelVal = null;
                long? cancelOrders = null;
                DateTime? sysTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0:  tradeDate     = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1:  tradeTime     = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2:  secId         = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3:  putOrdersB    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4:  putOrdersS    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5:  putValB       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6:  putValS       = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7:  putVolB       = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8:  putVolS       = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9:  putVwapB      = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 10: putVwapS      = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 11: putVol        = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 12: putVal        = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 13: putOrders     = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 14: cancelOrdersB = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 15: cancelOrdersS = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 16: cancelValB    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 17: cancelValS    = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 18: cancelVolB    = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 19: cancelVolS    = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 20: cancelVwapB   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 21: cancelVwapS   = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 22: cancelVol     = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 23: cancelVal     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 24: cancelOrders  = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 25: sysTime       = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new SuperCandlesOrderStats5mDTO
                {
                    TradeDate     = tradeDate,
                    TradeTime     = tradeTime,
                    SecId         = secId,
                    PutOrdersB    = putOrdersB,
                    PutOrdersS    = putOrdersS,
                    PutValB       = putValB,
                    PutValS       = putValS,
                    PutVolB       = putVolB,
                    PutVolS       = putVolS,
                    PutVwapB      = putVwapB,
                    PutVwapS      = putVwapS,
                    PutVol        = putVol,
                    PutVal        = putVal,
                    PutOrders     = putOrders,
                    CancelOrdersB = cancelOrdersB,
                    CancelOrdersS = cancelOrdersS,
                    CancelValB    = cancelValB,
                    CancelValS    = cancelValS,
                    CancelVolB    = cancelVolB,
                    CancelVolS    = cancelVolS,
                    CancelVwapB   = cancelVwapB,
                    CancelVwapS   = cancelVwapS,
                    CancelVol     = cancelVol,
                    CancelVal     = cancelVal,
                    CancelOrders  = cancelOrders,
                    SysTime       = sysTime,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // HI2 Stock (акции) — 7 колонок (B5)
        // ═══════════════════════════════════════════════════════════

        public static List<Hi2AssetDTO> ParseHi2Stock(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseHi2Stock(jsonBytes, out _);
        }

        public static List<Hi2AssetDTO> ParseHi2Stock(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.Hi2AssetSchema;
            var list = new List<Hi2AssetDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");
                    foundData = true;
                    ReadHi2StockData(ref reader, list, schema);
                }
                else { reader.Skip(); }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadHi2StockData(
            ref Utf8JsonReader reader,
            List<Hi2AssetDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null, metric = null, reference = null;
                double? value = null;
                DateTime? sysTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: tradeDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: tradeTime = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: secId     = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: metric    = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: value     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: reference = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: sysTime   = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new Hi2AssetDTO
                {
                    TradeDate = tradeDate,
                    TradeTime = tradeTime,
                    SecId     = secId,
                    Metric    = metric,
                    Value     = value,
                    Reference = reference,
                    SysTime   = sysTime,
                });
                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // HI2 Futures (фьючерсы) — 8 колонок (B5)
        // ═══════════════════════════════════════════════════════════

        public static List<Hi2FuturesDTO> ParseHi2Futures(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseHi2Futures(jsonBytes, out _);
        }

        public static List<Hi2FuturesDTO> ParseHi2Futures(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.Hi2FuturesSchema;
            var list = new List<Hi2FuturesDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");
                    foundData = true;
                    ReadHi2FuturesData(ref reader, list, schema);
                }
                else { reader.Skip(); }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadHi2FuturesData(
            ref Utf8JsonReader reader,
            List<Hi2FuturesDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null, assetCode = null;
                string? metric = null, reference = null;
                double? value = null;
                DateTime? sysTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: tradeDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: tradeTime = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: secId     = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: assetCode = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: metric    = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: value     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: reference = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: sysTime   = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new Hi2FuturesDTO
                {
                    TradeDate = tradeDate,
                    TradeTime = tradeTime,
                    SecId     = secId,
                    AssetCode = assetCode,
                    Metric    = metric,
                    Value     = value,
                    Reference = reference,
                    SysTime   = sysTime,
                });
                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MegaAlerts Stock (акции) — 8 колонок (B5)
        // ═══════════════════════════════════════════════════════════

        public static List<MegaAlertsAssetsDTO> ParseMegaAlertsStock(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseMegaAlertsStock(jsonBytes, out _);
        }

        public static List<MegaAlertsAssetsDTO> ParseMegaAlertsStock(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.MegaAlertsAssetSchema;
            var list = new List<MegaAlertsAssetsDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");
                    foundData = true;
                    ReadMegaAlertsStockData(ref reader, list, schema);
                }
                else { reader.Skip(); }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadMegaAlertsStockData(
            ref Utf8JsonReader reader,
            List<MegaAlertsAssetsDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null, alertType = null, reference = null;
                double? threshold = null, value = null;
                DateTime? sysTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: tradeDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: tradeTime = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: secId     = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: alertType = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: threshold = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: value     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: reference = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: sysTime   = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new MegaAlertsAssetsDTO
                {
                    TradeDate = tradeDate,
                    TradeTime = tradeTime,
                    SecId     = secId,
                    AlertType = alertType,
                    Threshold = threshold,
                    Value     = value,
                    Reference = reference,
                    SysTime   = sysTime,
                });
                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // MegaAlerts Futures (фьючерсы) — 9 колонок (B5)
        // ═══════════════════════════════════════════════════════════

        public static List<MegaAlertsFuturesDTO> ParseMegaAlertsFutures(ReadOnlySpan<byte> jsonBytes)
        {
            return ParseMegaAlertsFutures(jsonBytes, out _);
        }

        public static List<MegaAlertsFuturesDTO> ParseMegaAlertsFutures(
            ReadOnlySpan<byte> jsonBytes,
            out PaginationCursorDTO cursor)
        {
            var schema = ColumnAndNumbersForParsing.MegaAlertsFuturesSchema;
            var list = new List<MegaAlertsFuturesDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");
                    foundData = true;
                    ReadMegaAlertsFuturesData(ref reader, list, schema);
                }
                else { reader.Skip(); }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);

            // ── Phase 3: cursor ──
            cursor = new PaginationCursorDTO();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;
                if (reader.ValueTextEquals("data.cursor"u8))
                {
                    cursor = ParseHelpersUtf8.ReadCursorRootObject(ref reader, "data.cursor");
                    break;
                }
                reader.Skip();
            }

            return list;
        }

        private static void ReadMegaAlertsFuturesData(
            ref Utf8JsonReader reader,
            List<MegaAlertsFuturesDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? tradeDate = null, tradeTime = null, secId = null, assetCode = null;
                string? alertType = null, reference = null;
                double? threshold = null, value = null;
                DateTime? sysTime = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: tradeDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: tradeTime = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: secId     = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: assetCode = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: alertType = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: threshold = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: value     = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7: reference = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8: sysTime   = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new MegaAlertsFuturesDTO
                {
                    TradeDate = tradeDate,
                    TradeTime = tradeTime,
                    SecId     = secId,
                    AssetCode = assetCode,
                    AlertType = alertType,
                    Threshold = threshold,
                    Value     = value,
                    Reference = reference,
                    SysTime   = sysTime,
                });
                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // FUTOI — 13 колонок (B6)
        // ═══════════════════════════════════════════════════════════

        public static List<FutoiDTO> ParseFutoi(ReadOnlySpan<byte> jsonBytes)
        {
            var schema = ColumnAndNumbersForParsing.FutoiSchema;
            var list = new List<FutoiDTO>();
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
                        ParseHelpersUtf8.SchemaMismatch(schema.RootKey,
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. Порядок columns → data обязателен.");
                    foundData = true;
                    ReadFutoiData(ref reader, list, schema);
                }
                else { reader.Skip(); }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            return list;
        }

        private static void ReadFutoiData(
            ref Utf8JsonReader reader,
            List<FutoiDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                int? sessId = null, seqNum = null;
                string? tradeDate = null, tradeTime = null, ticker = null, clGroup = null;
                long? pos = null, posLong = null, posShort = null, posLongNum = null, posShortNum = null;
                DateTime? sysTime = null;
                string? tradeSessionDate = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0:  sessId           = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1:  seqNum           = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2:  tradeDate        = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3:  tradeTime        = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4:  ticker           = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5:  clGroup          = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6:  pos              = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7:  posLong          = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 8:  posShort         = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9:  posLongNum       = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 10: posShortNum      = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 11: sysTime          = ParseHelpersUtf8.ReadDateTimeUtf8(ref r, rowIndex, idx, schema.RootKey); break;
                            case 12: tradeSessionDate = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new FutoiDTO
                {
                    SessId           = sessId,
                    SeqNum           = seqNum,
                    TradeDate        = tradeDate,
                    TradeTime        = tradeTime,
                    Ticker           = ticker,
                    ClGroup          = clGroup,
                    Pos              = pos,
                    PosLong          = posLong,
                    PosShort         = posShort,
                    PosLongNum       = posLongNum,
                    PosShortNum      = posShortNum,
                    SysTime          = sysTime,
                    TradeSessionDate = tradeSessionDate,
                });
                rowIndex++;
            }
        }
    }
}
