using System.Text;
using System.Text.Json;
using History_DataMoex.Parsing.Errors;

namespace History_DataMoex.Parsing.ColumnIndex
{
    /// <summary>
    /// Общий resolver индексов колонок MOEX для формата columns[] + data[][].
    /// Пока не используется существующими парсерами напрямую.
    /// Нужен для будущего безопасного рефакторинга ParsingISS, ParsingALG и ParsingCalendar.
    /// </summary>
    public static class MoexColumnIndexResolver
    {
        /// <summary>
        /// Разрешает индексы ожидаемых колонок по блоку MOEX columns[].
        /// Для необязательных ненайденных колонок возвращает -1.
        /// Для обязательных ненайденных колонок выбрасывает <see cref="MoexSchemaMismatchException"/>.
        /// </summary>
        /// <param name="columnsArray">Блок columns[] из ответа MOEX.</param>
        /// <param name="expectedUtf8">Ожидаемые имена колонок в UTF-8.</param>
        /// <param name="required">Флаги обязательности колонок.</param>
        /// <param name="sourceCode">Код источника данных.</param>
        /// <param name="dataNeedCode">Код типа запрашиваемых данных.</param>
        /// <param name="endpoint">Имя или путь endpoint.</param>
        /// <param name="rawObjectId">Идентификатор сырого объекта, если доступен.</param>
        /// <returns>Массив индексов длиной expectedUtf8.Length.</returns>
        /// <exception cref="ArgumentException">Выбрасывается, когда длины expectedUtf8 и required не совпадают.</exception>
        /// <exception cref="MoexSchemaMismatchException">Выбрасывается, когда columns[] не является массивом или отсутствуют обязательные колонки.</exception>
        public static int[] Resolve(
            JsonElement columnsArray,
            ReadOnlySpan<byte[]> expectedUtf8,
            ReadOnlySpan<bool> required,
            string? sourceCode = null,
            string? dataNeedCode = null,
            string? endpoint = null,
            Guid? rawObjectId = null)
        {
            if (expectedUtf8.Length != required.Length)
            {
                throw new ArgumentException("expectedUtf8 and required must have the same length.");
            }

            string[] expectedColumns = new string[expectedUtf8.Length];
            for (int i = 0; i < expectedUtf8.Length; i++)
            {
                expectedColumns[i] = Encoding.UTF8.GetString(expectedUtf8[i]);
            }

            if (columnsArray.ValueKind != JsonValueKind.Array)
            {
                List<string> missingColumns = new(expectedUtf8.Length);
                for (int i = 0; i < expectedColumns.Length; i++)
                {
                    if (required[i])
                    {
                        missingColumns.Add(expectedColumns[i]);
                    }
                }

                throw new MoexSchemaMismatchException(
                    "MOEX columns block is not an array.",
                    expectedColumns,
                    Array.Empty<string>(),
                    missingColumns,
                    sourceCode,
                    dataNeedCode,
                    endpoint,
                    rawObjectId);
            }

            int[] indexes = new int[expectedUtf8.Length];
            Array.Fill(indexes, -1);

            List<string> actualColumns = new(columnsArray.GetArrayLength());
            for (int i = 0; i < columnsArray.GetArrayLength(); i++)
            {
                JsonElement actualColumn = columnsArray[i];
                actualColumns.Add(actualColumn.GetString() ?? string.Empty);

                for (int j = 0; j < expectedUtf8.Length; j++)
                {
                    if (indexes[j] != -1)
                    {
                        continue;
                    }

                    if (actualColumn.ValueEquals(expectedUtf8[j]))
                    {
                        indexes[j] = i;
                    }
                }
            }

            List<string> missingRequiredColumns = new();
            for (int i = 0; i < indexes.Length; i++)
            {
                if (required[i] && indexes[i] == -1)
                {
                    missingRequiredColumns.Add(expectedColumns[i]);
                }
            }

            if (missingRequiredColumns.Count > 0)
            {
                throw new MoexSchemaMismatchException(
                    $"MOEX required columns are missing: {string.Join(", ", missingRequiredColumns)}.",
                    expectedColumns,
                    actualColumns,
                    missingRequiredColumns,
                    sourceCode,
                    dataNeedCode,
                    endpoint,
                    rawObjectId);
            }

            return indexes;
        }
    }
}