using System.Globalization;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class ParseHelpersUtf8
    {
        public static DateTime? GetDateTimeOrNull(string dateTime)
        {
            if (dateTime is null)
            {
                return null;
            }
            if (DateTime.TryParseExact(dateTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                return dt;
            }
            return null;
        }
    }
}
