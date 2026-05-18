using History_DataMoex.Contracts.Dto.Iss;
using System.Globalization;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public static class ParsingIssUtf8
    {
        // ═══════════════════════════════════════════════════════════
        // ISS Securities — Акции (фондовый рынок)
        // 9 из 27 колонок, rootKey: "securities"
        // SourceIndex с пропусками: 0,1,2,4,5,9,11,17,22
        // ═══════════════════════════════════════════════════════════

        public static List<StockSecurityDTO> ParseIssSecurityStock(ReadOnlySpan<byte> jsonBytes)
        {
            var schema = ColumnAndNumbersForParsing.IssStockSecuritySchema;
            var list = new List<StockSecurityDTO>();
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
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. " +
                            $"Порядок columns → data обязателен.");

                    foundData = true;
                    ReadIssSecurityStockData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            return list;
        }

        private static void ReadIssSecurityStockData(
            ref Utf8JsonReader reader,
            List<StockSecurityDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? secId = null, boardId = null, shortName = null, secName = null, marketCode = null;
                int? lotSize = null;
                double? faceValue = null;
                DateTime? prevDate = null;
                double? prevLegalClosePriceRaw = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: secId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: boardId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: shortName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: lotSize = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: faceValue = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: secName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6: marketCode = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 7:
                                string? prevDateStr = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey);
                                if (prevDateStr != null && DateTime.TryParseExact(prevDateStr, "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None, out var dt))
                                    prevDate = dt;
                                break;
                            case 8: prevLegalClosePriceRaw = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new StockSecurityDTO
                {
                    SECID = secId,
                    BOARDID = boardId,
                    SHORTNAME = shortName,
                    LOTSIZE = lotSize,
                    FACEVALUE = faceValue,
                    SECNAME = secName,
                    MARKETCODE = marketCode,
                    PREVDATE = prevDate,
                    PREVLEGALCLOSEPRICE = (decimal?)prevLegalClosePriceRaw,
                });

                rowIndex++;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // ISS Securities — Фьючерсы (срочный рынок)
        // 16 из 26 колонок, rootKey: "securities"
        // SourceIndex с пропусками: 0,2,3,4,5,6,7,8,11,12,13,14,15,16,17,19
        // ═══════════════════════════════════════════════════════════

        public static List<FuturesSecurityDTO> ParseIssSecurityFutures(ReadOnlySpan<byte> jsonBytes)
        {
            var schema = ColumnAndNumbersForParsing.IssFuturesSecuritySchema;
            var list = new List<FuturesSecurityDTO>();
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
                            $"[{schema.RootKey}] Секция 'data' встретилась до 'columns'. " +
                            $"Порядок columns → data обязателен.");

                    foundData = true;
                    ReadIssSecurityFuturesData(ref reader, list, schema);
                }
                else
                {
                    reader.Skip();
                }
            }

            ParseHelpersUtf8.ValidateStructure(foundColumns, foundData, schema.RootKey);
            return list;
        }

        private static void ReadIssSecurityFuturesData(
            ref Utf8JsonReader reader,
            List<FuturesSecurityDTO> list,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

            int rowIndex = 0;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                string? secId = null, shortName = null, secName = null, assetCode = null;
                double? prevSettlePrice = null, minStep = null, initialMargin = null;
                double? highLimit = null, lowLimit = null, stepPrice = null, prevPrice = null;
                int? decimals = null, lotVolume = null;
                DateTime? lastTradeDate = null, lastDelDate = null;
                long? prevOpenPosition = null;

                ParseHelpersUtf8.ReadDataRow(ref reader, schema, rowIndex,
                    (ref Utf8JsonReader r, int idx) =>
                    {
                        switch (idx)
                        {
                            case 0: secId = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 1: shortName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 2: secName = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 3: prevSettlePrice = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 4: decimals = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 5: minStep = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 6:
                                string? lastTradeDateStr = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey);
                                if (lastTradeDateStr != null && DateTime.TryParseExact(lastTradeDateStr, "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt6))
                                    lastTradeDate = dt6;
                                break;
                            case 7:
                                string? lastDelDateStr = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey);
                                if (lastDelDateStr != null && DateTime.TryParseExact(lastDelDateStr, "yyyy-MM-dd",
                                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt7))
                                    lastDelDate = dt7;
                                break;
                            case 8: assetCode = ParseHelpersUtf8.ReadString(ref r, rowIndex, idx, schema.RootKey); break;
                            case 9: prevOpenPosition = ParseHelpersUtf8.ReadLong(ref r, rowIndex, idx, schema.RootKey); break;
                            case 10: lotVolume = ParseHelpersUtf8.ReadInt(ref r, rowIndex, idx, schema.RootKey); break;
                            case 11: initialMargin = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 12: highLimit = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 13: lowLimit = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 14: stepPrice = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                            case 15: prevPrice = ParseHelpersUtf8.ReadDouble(ref r, rowIndex, idx, schema.RootKey); break;
                        }
                    });

                list.Add(new FuturesSecurityDTO
                {
                    SECID = secId,
                    SHORTNAME = shortName,
                    SECNAME = secName,
                    PREVSETTLEPRICE = prevSettlePrice,
                    DECIMALS = decimals,
                    MINSTEP = minStep,
                    LASTTRADEDATE = lastTradeDate,
                    LASTDELDATE = lastDelDate,
                    ASSETCODE = assetCode,
                    PREVOPENPOSITION = prevOpenPosition,
                    LOTVOLUME = lotVolume,
                    INITIALMARGIN = initialMargin,
                    HIGHLIMIT = highLimit,
                    LOWLIMIT = lowLimit,
                    STEPPRICE = stepPrice,
                    PREVPRICE = prevPrice,
                });

                rowIndex++;
            }
        }
    }
}
