using System.Globalization;
using History_DataMoex.Mappers.Errors;

namespace History_DataMoex.Mappers;

/// <summary>
/// Парсер для отдельных date-only / time-only / MSK→UTC timestamp полей Calendar и ISS.
/// Не путать с TradeDateTimeParser — тот парсит combined ALGOPACK tradeDate + tradeTime.
/// Бросает MappingDateTimeException с FieldName/RawValue при invalid format.
/// Для nullable source полей возвращает null при null/empty входе, бросает только при invalid format.
/// Для required полей строгий — null/empty/invalid → throw.
/// </summary>
public static class SourceDateTimeParser
{
    // Форматы — единичный массив элементов для DateOnly (MOEX даёт только "yyyy-MM-dd").
    // Для TimeOnly допустимы оба HH:mm:ss и HH:mm (последний на случай session-полей).
    // Для DateTime — base + fractional seconds 1..7 знаков (Spec v3 §10.1).
    // Source MOEX иногда даёт ".F" одну цифру, иногда ".FFFFFFF" — все вариации покрыты.
    private static readonly string[] DateOnlyFormats = { "yyyy-MM-dd" };
    private static readonly string[] TimeOnlyFormats = { "HH:mm:ss", "HH:mm" };
    private static readonly string[] DateTimeFormats =
    {
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd H:mm:ss",
        "yyyy-MM-dd HH:mm:ss.F",
        "yyyy-MM-dd HH:mm:ss.FF",
        "yyyy-MM-dd HH:mm:ss.FFF",
        "yyyy-MM-dd HH:mm:ss.FFFF",
        "yyyy-MM-dd HH:mm:ss.FFFFF",
        "yyyy-MM-dd HH:mm:ss.FFFFFF",
        "yyyy-MM-dd HH:mm:ss.FFFFFFF",
    };

    /// <summary>
    /// Парсит дату формата "yyyy-MM-dd". null/empty → null. Invalid format → MappingDateTimeException.
    /// Используется для nullable Calendar/ISS date полей (ExpirationDate, EndDate, и т.д.).
    /// </summary>
    public static DateOnly? ParseDateOnlyOrNull(string? raw, string fieldName)
    {
        if (string.IsNullOrEmpty(raw))
            return null;

        if (!DateOnly.TryParseExact(raw, DateOnlyFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly result))
        {
            throw MappingDateTimeException.ForField(
                $"Cannot parse field '{fieldName}' as DateOnly: '{raw}'",
                fieldName: fieldName,
                rawValue: raw);
        }

        return result;
    }

    /// <summary>
    /// Строгий парсер: null/empty → throw MappingDateTimeException, invalid format → throw.
    /// Используется для required бизнес-ключевых date полей (TradeDate в OffDayAll и др.).
    /// </summary>
    public static DateOnly ParseRequiredDateOnly(string? raw, string fieldName)
    {
        if (string.IsNullOrEmpty(raw))
        {
            throw MappingDateTimeException.ForField(
                $"Required field '{fieldName}' is null or empty (expected DateOnly)",
                fieldName: fieldName,
                rawValue: raw);
        }

        DateOnly? parsed = ParseDateOnlyOrNull(raw, fieldName);
        return parsed!.Value;
    }

    /// <summary>
    /// Парсит время "HH:mm:ss" или "HH:mm". null/empty → null. Invalid format → throw.
    /// Используется для time-only полей stock session (TimeFrom, TimeTill приходят как string).
    /// </summary>
    public static TimeOnly? ParseTimeOnlyOrNull(string? raw, string fieldName)
    {
        if (string.IsNullOrEmpty(raw))
            return null;

        if (!TimeOnly.TryParseExact(raw, TimeOnlyFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out TimeOnly result))
        {
            throw MappingDateTimeException.ForField(
                $"Cannot parse field '{fieldName}' as TimeOnly: '{raw}'",
                fieldName: fieldName,
                rawValue: raw);
        }

        return result;
    }

    /// <summary>
    /// Конвертирует DateTime? в MSK timezone в UTC. null → null.
    /// DateTimeKind.Unspecified трактуется как Europe/Moscow (source timezone — Lock §4).
    /// DateTimeKind.Utc → возвращается как есть.
    /// DateTimeKind.Local → throw (ambiguous, machine-dependent).
    /// Используется для полей UpdateTime, ChangeTime, TimeFrom/TimeTill (futures session — full timestamp).
    /// </summary>
    public static DateTime? ParseMskDateTimeToUtcOrNull(DateTime? raw, TimeZoneInfo sourceTz, string fieldName)
    {
        if (!raw.HasValue)
            return null;

        DateTime value = raw.Value;
        DateTime local;

        switch (value.Kind)
        {
            case DateTimeKind.Utc:
                return value;
            case DateTimeKind.Unspecified:
                // источник: Europe/Moscow (Lock §4)
                local = value;
                break;
            case DateTimeKind.Local:
                throw MappingDateTimeException.ForField(
                    $"Field '{fieldName}' has DateTimeKind.Local, which is ambiguous (machine-dependent). " +
                    "Source DateTime must be Unspecified (treated as Europe/Moscow) or Utc.",
                    fieldName: fieldName,
                    rawValue: value.ToString("O", CultureInfo.InvariantCulture));
            default:
                throw MappingDateTimeException.ForField(
                    $"Field '{fieldName}' has unexpected DateTimeKind: {value.Kind}",
                    fieldName: fieldName,
                    rawValue: value.ToString("O", CultureInfo.InvariantCulture));
        }

        return TimeZoneInfo.ConvertTimeToUtc(local, sourceTz);
    }

    /// <summary>
    /// Парсит timestamp-строку MSK в UTC. null/empty → null. Invalid format → throw.
    /// Используется когда DTO даёт string? (а не DateTime?) для timestamp поля.
    /// Принимает форматы из DateTimeFormats (base + fractional seconds 1..7 знаков).
    /// </summary>
    public static DateTime? ParseMskDateTimeStringToUtcOrNull(string? raw, TimeZoneInfo sourceTz, string fieldName)
    {
        if (string.IsNullOrEmpty(raw))
            return null;

        if (!DateTime.TryParseExact(raw, DateTimeFormats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
        {
            throw MappingDateTimeException.ForField(
                $"Cannot parse field '{fieldName}' as DateTime: '{raw}'",
                fieldName: fieldName,
                rawValue: raw);
        }

        // Явно помечаем Unspecified, чтобы ConvertTimeToUtc интерпретировал как source-tz.
        DateTime local = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, sourceTz);
    }
}
