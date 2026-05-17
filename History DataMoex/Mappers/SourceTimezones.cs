using History_DataMoex.Mappers.Errors;

namespace History_DataMoex.Mappers;

public static class SourceTimezones
{
    /// <summary>
    /// Fixed +03:00 для Europe/Moscow на случай отсутствия системной tzdb (Alpine, slim containers).
    /// ВАЖНО: корректен только для данных MOEX после 2014-10-26 — постоянный UTC+3 без DST.
    /// ALGOPACK даёт максимум 2 года истории, проблемы не возникает.
    /// </summary>
    private static readonly TimeZoneInfo MskFixed = TimeZoneInfo.CreateCustomTimeZone(
        id: "MSK_fixed",
        baseUtcOffset: TimeSpan.FromHours(3),
        displayName: "MSK (fixed +03:00, valid since 2014-10-26)",
        standardDisplayName: "MSK");

    public static TimeZoneInfo Resolve(string ianaId)
    {
        if (TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out TimeZoneInfo? tz))
            return tz;

        if (ianaId == "Europe/Moscow")
            return MskFixed;

        throw new MappingTimezoneException(ianaId);
    }
}
