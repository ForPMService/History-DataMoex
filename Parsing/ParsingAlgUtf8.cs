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
                        throw new InvalidOperationException(
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. " +
                            $"Порядок columns → data обязателен.");
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
                        throw new InvalidOperationException(
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
                        throw new InvalidOperationException(
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
    }
}
