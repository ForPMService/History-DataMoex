using System.Globalization;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    internal static class ParseHelpers
    {
        public static string? GetStringOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
            return null;
        }
        public static decimal? GetDecimalOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDecimal();
            }
            return null;
        }
        public static double? GetDoubleOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDouble();
            }
            return null;
        }

        public static long? GetLongOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetInt64();
            }
            return null;
        }

        public static int? GetIntOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetInt32();
            }
            return null;
        }

        public static DateTime? GetDateTimeOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParseExact(element.GetString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                {
                    return dateTime;
                }
            }
            return null;
        }

       

    }
}
