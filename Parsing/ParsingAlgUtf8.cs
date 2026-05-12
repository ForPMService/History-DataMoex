using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Parsing.Errors;
using System.Globalization;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class ParsingAlgUtf8
    {
        /// <summary>
        /// Парсеры MOEX ALGOPACK на Utf8JsonReader.
        /// 
        /// Без JsonDocument, без дерева в памяти.
        /// Принимают ReadOnlySpan&lt;byte&gt; — сырые байты JSON-ответа.
        /// 
        /// Паттерн для каждого парсера:
        /// 1. Найти корневой ключ (rootKey) — "candles", "data", "futoi".
        /// 2. Внутри корневого ключа найти "columns" → проверить порядок и количество.
        /// 3. Найти "data" → прочитать массив массивов, собрать DTO.
        /// 4. Если что-то не найдено или не совпало — бросить ошибку с контекстом.
        /// </summary>

        // ═══════════════════════════════════════════════════════════
        // Свечи (акции и фьючерсы)
        // ═══════════════════════════════════════════════════════════

        public static List<CandlesDTO> ParseAlgCandles(ReadOnlySpan<byte> jsonBytes)
        {
            var schema = ColumnAndNumbersForParsing.AlgCandlesSchema;
            var candlesList = new List<CandlesDTO>();
            var reader = new Utf8JsonReader(jsonBytes);

            // ── Шаг 1. Найти RootKey на верхнем уровне JSON ──
            //
            // Структура ответа MOEX:
            // {
            //   "candles": {            ← ищем это свойство
            //     "columns": [...],
            //     "data": [[...], ...]
            //   }
            // }
            //
            // На верхнем уровне могут быть и другие свойства ("metadata" и т.д.) —
            // их пропускаем через Skip(), чтобы не подхватить чужие "columns"/"data".

            ParseHelpersUtf8.SkipToRootObject(ref reader, schema.RootKey);

            // ── Шаг 2. Читать свойства ТОЛЬКО внутри RootKey-объекта ──
            //
            // reader сейчас стоит на StartObject внутри "candles".
            // Читаем его свойства: "columns", "data".
            // Всё неизвестное — Skip().
            // Выходим по EndObject.

            bool foundColumns = false;
            bool foundData = false;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    // Вышли из RootKey-объекта — дальше читать не нужно.
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                if (reader.ValueTextEquals("columns"u8))
                {
                    foundColumns = true;
                    ParseHelpersUtf8.ValidateColumnsUtf8(ref reader, schema);
                }
                else if (reader.ValueTextEquals("data"u8))
                {
                    foundData = true;
                    ReadCandlesData(ref reader, candlesList, schema);
                }
                else
                {
                    // Неизвестное свойство внутри RootKey — пропускаем.
                    // Например, если MOEX добавит "candles.cursor" или "candles.metadata".
                    reader.Skip();
                }
            }

            // ── Шаг 3. Проверить что нашли обязательные секции ──

            ParseHelpersUtf8.ValidateStructure(
                foundRoot: true, // SkipToRootObject бросил бы, если не нашёл
                foundColumns,
                foundData,
                schema.RootKey);

            return candlesList;
        }

        /// <summary>
        /// Чтение массива строк данных свечей.
        /// 
        /// Структура JSON:
        /// "data": [
        ///   [85.54, 85.76, 86.17, 85.54, 424470125.4, 4944070, "2011-12-08 10:00:00", "2011-12-08 10:09:59"],
        ///   ...
        /// ]
        /// </summary>
        //private static void ReadCandlesData(
        //        ref Utf8JsonReader reader,
        //        List<CandlesDTO> candlesList,
        //        ColumnAndNumbersForParsing.ExpectedSchema schema)
        //    {
        //        ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.StartArray, "data", schema.RootKey);

        //        int rowIndex = 0;
        //        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        //        {
        //            // reader стоит на StartArray внутреннего массива (одна строка)
        //            double? open = null, close = null, high = null, low = null;
        //            double? value = null, volume = null;
        //            DateTime? begin = null, end = null;

        //            for (int i = 0; i < schema.Columns.Length; i++)
        //            {
        //                if (!reader.Read())
        //                    throw new InvalidOperationException(
        //                        $"[{schema.RootKey}] Неожиданный конец JSON в строке {rowIndex}, колонка {i}.");

        //                if (reader.TokenType == JsonTokenType.Null)
        //                    continue;

        //                switch (i)
        //                {
        //                    case 0: open = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
        //                    case 1: close = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
        //                    case 2: high = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
        //                    case 3: low = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
        //                    case 4: value = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
        //                    case 5: volume = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
        //                    case 6: begin = ParseHelpersUtf8.ReadDateTime(ref reader, rowIndex, i, schema.RootKey); break;
        //                    case 7: end = ParseHelpersUtf8.ReadDateTime(ref reader, rowIndex, i, schema.RootKey); break;
        //                }
        //            }

        //            ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.EndArray, $"data row {rowIndex}", schema.RootKey);

        //            candlesList.Add(new CandlesDTO
        //            {
        //                Open = open,
        //                Close = close,
        //                High = high,
        //                Low = low,
        //                Value = value,
        //                Volume = volume,
        //                Begin = begin,
        //                End = end
        //            });

        //            rowIndex++;
        //        }
        //    }

            
        //}
    }
