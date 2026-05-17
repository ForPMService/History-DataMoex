using System.Globalization;
using History_DataMoex.Mappers.Errors;

namespace History_DataMoex.Mappers;

public static class TradeDateTimeParser
{
    private static readonly string[] Formats =
    {
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd H:mm:ss",
        "yyyy-MM-dd HH:mm:ss.FFF",
        "yyyy-MM-dd H:mm:ss.FFF",
    };

    public static DateTime Parse(string? tradeDate, string? tradeTime)
    {
        if (string.IsNullOrEmpty(tradeDate))
            throw new MappingDateTimeException(
                "TradeDate is null or empty",
                rawTradeDate: tradeDate,
                rawTradeTime: tradeTime);
        if (string.IsNullOrEmpty(tradeTime))
            throw new MappingDateTimeException(
                "TradeTime is null or empty",
                rawTradeDate: tradeDate,
                rawTradeTime: tradeTime);

        string combined = $"{tradeDate} {tradeTime}";

        if (!DateTime.TryParseExact(combined, Formats,
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
        {
            throw new MappingDateTimeException(
                $"Cannot parse TradeDate+TradeTime: '{combined}'",
                rawTradeDate: tradeDate,
                rawTradeTime: tradeTime);
        }

        return DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
    }

    public static (DateTime utc, DateTime local) ParseToUtc(
        string? tradeDate, string? tradeTime, TimeZoneInfo sourceTz)
    {
        DateTime local = Parse(tradeDate, tradeTime);
        DateTime utc = TimeZoneInfo.ConvertTimeToUtc(local, sourceTz);
        return (utc, local);
    }
}
