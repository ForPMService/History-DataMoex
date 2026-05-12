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
    }
}
