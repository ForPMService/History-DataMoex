using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Parsing.Errors;
using System.Globalization;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class ParseHelpersUtf8
    {
        // ═══════════════════════════════════════════════════════════
        // Общие методы — переиспользуются всеми парсерами
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Проверка массива columns[] — порядок и количество.
        /// 
        /// Читает StartArray, проходит по строковым токенам,
        /// сравнивает каждый с ожидаемым именем через ValueTextEquals (без аллокаций).
        /// 
        /// Для схем без пропусков (все ALGOPACK, Calendar) — проверяет все колонки подряд.
        /// Для схем с пропусками (ISS Securities) — проверяет только указанные позиции.
        /// 
        /// Бросает MoexSchemaMismatchException при несовпадении имени колонки.
        /// Бросает InvalidOperationException при неправильном количестве колонок.
        /// </summary>
        internal static void ValidateColumnsUtf8(
            ref Utf8JsonReader reader,
            ColumnAndNumbersForParsing.ExpectedSchema schema)
        {
            ReadAndExpect(ref reader, JsonTokenType.StartArray, "columns", schema.RootKey);

            int position = 0;
            int expectedIdx = 0; // индекс в массиве schema.Columns

            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                // Проверяем только те позиции, которые есть в schema.Columns
                if (expectedIdx < schema.Columns.Length
                    && position == schema.Columns[expectedIdx].SourceIndex)
                {
                    if (!reader.ValueTextEquals(schema.Columns[expectedIdx].Name))
                    {
                        string actual = reader.GetString() ?? "<null>";
                        string expected = System.Text.Encoding.UTF8.GetString(
                            schema.Columns[expectedIdx].Name);

                        throw new MoexSchemaMismatchException(
                            $"[{schema.RootKey}] Колонка не совпала на позиции {position}: " +
                            $"ожидалось '{expected}', получено '{actual}'.",
                            expectedColumns: schema.Columns
                                .Select(c => System.Text.Encoding.UTF8.GetString(c.Name))
                                .ToList(),
                            actualColumns: new List<string> { actual },
                            missingColumns: new List<string> { expected },
                            dataNeedCode: schema.RootKey);
                    }
                    expectedIdx++;
                }
                position++;
            }

            // Проверка общего количества колонок
            if (position != schema.TotalColumns)
                throw new InvalidOperationException(
                    $"[{schema.RootKey}] Количество колонок не совпадает: " +
                    $"ожидалось {schema.TotalColumns}, получено {position}.");

            // Проверка что все ожидаемые колонки были найдены
            if (expectedIdx != schema.Columns.Length)
                throw new InvalidOperationException(
                    $"[{schema.RootKey}] Не все ожидаемые колонки найдены: " +
                    $"ожидалось {schema.Columns.Length}, проверено {expectedIdx}.");
        }

        /// <summary>
        /// Проверка что парсер нашёл все обязательные секции в JSON.
        /// Вызывается после завершения основного цикла чтения.
        /// </summary>
        internal static void ValidateStructure(
            bool foundRoot,
            bool foundColumns,
            bool foundData,
            string rootKey)
        {
            if (!foundRoot)
                throw new InvalidOperationException(
                    $"MOEX ответ не содержит корневой ключ '{rootKey}'.");

            if (!foundColumns)
                throw new InvalidOperationException(
                    $"[{rootKey}] Блок не содержит секцию 'columns'.");

            if (!foundData)
                throw new InvalidOperationException(
                    $"[{rootKey}] Блок не содержит секцию 'data'.");
        }

        /// <summary>
        /// Прочитать следующий токен и проверить что он нужного типа.
        /// Используется для StartArray/EndArray — мест, где тип токена фиксирован.
        /// </summary>
        internal static void ReadAndExpect(
            ref Utf8JsonReader reader,
            JsonTokenType expectedType,
            string context,
            string rootKey)
        {
            if (!reader.Read() || reader.TokenType != expectedType)
                throw new InvalidOperationException(
                    $"[{rootKey}] Ожидался {expectedType} для '{context}', " +
                    $"получено {reader.TokenType.ToString()}.");
        }

        /// <summary>
        /// Прочитать double из текущего токена.
        /// Если токен не Number — бросить ошибку с указанием строки и колонки.
        /// </summary>
        internal static double ReadDouble(
            ref Utf8JsonReader reader,
            int rowIndex,
            int columnIndex,
            string rootKey)
        {
            if (reader.TokenType != JsonTokenType.Number)
                throw new InvalidOperationException(
                    $"[{rootKey}] Ожидался Number в строке {rowIndex}, колонка {columnIndex}, " +
                    $"получено {reader.TokenType.ToString()}.");

            return reader.GetDouble();
        }

        /// <summary>
        /// Прочитать DateTime из текущего строкового токена.
        /// Формат MOEX: "yyyy-MM-dd HH:mm:ss" (всегда 19 символов, пробел вместо T).
        /// 
        /// Если токен не String — бросить ошибку.
        /// Если строка не парсится как дата — вернуть null (не падать,
        /// потому что MOEX может отдать пустую строку).
        /// </summary>
        internal static DateTime? ReadDateTime(
            ref Utf8JsonReader reader,
            int rowIndex,
            int columnIndex,
            string rootKey)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new InvalidOperationException(
                    $"[{rootKey}] Ожидался String (datetime) в строке {rowIndex}, колонка {columnIndex}, " +
                    $"получено {reader.TokenType.ToString()}.");

            string? value = reader.GetString();
            if (value is null)
                return null;

            if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt;
            }

            return null;
        }

        // ═══════════════════════════════════════════════════════════
        // Навигация к RootKey
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// На верхнем уровне JSON ищет свойство с именем rootKey,
        /// пропуская всё остальное через Skip().
        /// 
        /// После вызова reader стоит на StartObject внутри rootKey.
        /// 
        /// Бросает InvalidOperationException если:
        /// — rootKey не найден до конца JSON;
        /// — значение rootKey не является объектом.
        /// </summary>
        internal static void SkipToRootObject(
            ref Utf8JsonReader reader,
            string rootKey)
        {
            // Конвертируем rootKey в UTF-8 байты для сравнения без аллокаций.
            // Для однократного вызова на парсер — допустимо.
            ReadOnlySpan<byte> rootKeyUtf8 = System.Text.Encoding.UTF8.GetBytes(rootKey);

            // Пройти до StartObject верхнего уровня
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.StartObject)
                    break;
            }

            // Теперь внутри корневого объекта JSON — ищем свойство rootKey
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    // Дошли до конца корневого объекта — rootKey не найден
                    throw new InvalidOperationException(
                        $"MOEX ответ не содержит корневой ключ '{rootKey}'.");
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                if (reader.ValueTextEquals(rootKeyUtf8))
                {
                    // Нашли rootKey — читаем его значение, оно должно быть объектом
                    if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new InvalidOperationException(
                            $"[{rootKey}] Ожидался объект (StartObject), " +
                            $"получено {reader.TokenType}.");
                    }

                    // reader стоит на StartObject — готово
                    return;
                }

                // Не наш ключ — пропустить значение целиком
                reader.Skip();
            }

            // JSON закончился без rootKey
            throw new InvalidOperationException(
                $"MOEX ответ не содержит корневой ключ '{rootKey}'.");
        }

        // ═══════════════════════════════════════════════════════════
        // Чтение данных свечей
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Чтение массива строк данных свечей.
        /// 
        /// Структура JSON:
        /// "data": [
        ///   [85.54, 85.76, 86.17, 85.54, 424470125.4, 4944070, "2011-12-08 10:00:00", "2011-12-08 10:09:59"],
        ///   ...
        /// ]
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

                for (int i = 0; i < schema.Columns.Length; i++)
                {
                    if (!reader.Read())
                        throw new InvalidOperationException(
                            $"[{schema.RootKey}] Неожиданный конец JSON в строке {rowIndex}, колонка {i}.");

                    if (reader.TokenType == JsonTokenType.Null)
                        continue;

                    switch (i)
                    {
                        case 0: open = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
                        case 1: close = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
                        case 2: high = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
                        case 3: low = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
                        case 4: value = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
                        case 5: volume = ParseHelpersUtf8.ReadDouble(ref reader, rowIndex, i, schema.RootKey); break;
                        case 6: begin = ParseHelpersUtf8.ReadDateTime(ref reader, rowIndex, i, schema.RootKey); break;
                        case 7: end = ParseHelpersUtf8.ReadDateTime(ref reader, rowIndex, i, schema.RootKey); break;
                    }
                }

                ParseHelpersUtf8.ReadAndExpect(ref reader, JsonTokenType.EndArray, $"data row {rowIndex}", schema.RootKey);

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
