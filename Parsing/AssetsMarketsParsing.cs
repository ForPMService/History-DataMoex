using ClosedXML.Excel;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public class AssetsMarketsParsing
    {
        public AssetsMarketsParsing()
        {

        }

        public void CreateTable(string jsonstring)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonstring);
            var root = doc.RootElement;
            ParceStockSecurity(root);


        }

        public List<StockSecurity> ParceStockSecurity(JsonElement element)
        {
            List<StockSecurity> elements = new List<StockSecurity>();
            StockSecurity stockSecurities = new StockSecurity()
            {
                SECID = GetStringOrNull(element, "SECID"),
                SHORTNAME = GetStringOrNull(element, "SHORTNAME"),
                LATNAME = GetStringOrNull(element, "LATNAME"),
                SECNAME = GetStringOrNull(element, "SECNAME"),
                ISIN = GetStringOrNull(element, "ISIN"),
                CURRENCYID = GetStringOrNull(element, "CURRENCYID"),
                REGNUMBER = GetStringOrNull(element, "REGNUMBER"),
                BOARDID = GetStringOrNull(element, "BOARDID"),
                LOTSIZE = GetIntOrNull(element, "LOTSIZE"),
                FACEVALUE = GetDecimalOrNull(element, "FACEVALUE"),
                STATUS = GetStringOrNull(element, "STATUS"),
                BOARDNAME = GetStringOrNull(element, "BOARDNAME"),
                MINSTEP = GetDecimalOrNull(element, "MINSTEP"),
                LISTLEVEL = GetIntOrNull(element, "LISTLEVEL"),
                ISSUESIZE = GetLongOrNull(element, "ISSUESIZE"),
                PREVPRICE = GetDecimalOrNull(element, "PREVPRICE"),
                PREVWAPRICE = GetDecimalOrNull(element, "PREVWAPRICE"),
                PREVLEGALCLOSEPRICE = GetDecimalOrNull(element, "PREVLEGALCLOSEPRICE"),
                PREVDATE = GetStringOrNull(element, "PREVDATE"),
                SETTLEDATE = GetStringOrNull(element, "SETTLEDATE")
            };

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    // это объект — у него есть свойства, можно вызвать EnumerateObject()
                    foreach (JsonProperty prop in element.EnumerateObject())
                    {
                        Console.WriteLine($"Ключ: {prop.Name}");
                        ParceStockSecurity(prop.Value);  // ← рекурсия: обрабатываем значение свойства
                    }
                    break;

                case JsonValueKind.Array:
                    // это массив — у него есть элементы, можно вызвать EnumerateArray()
                    foreach (JsonElement item in element.EnumerateArray())
                    {
                        ParceStockSecurity(item);  // ← рекурсия: обрабатываем каждый элемент
                    }
                    break;

                default:
                    // это лист — простое значение, дальше спускаться некуда
                    Console.WriteLine($"Значение: {element}");
                    break;
            }
            return elements;

        }

        private static string? GetStringOrNull(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String)
            {
                return value.GetString();
            }
            return null;
        }
        private static decimal? GetDecimalOrNull(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.Number)
            {
                return value.GetDecimal();
            }
            return null;
        }

        private static long? GetLongOrNull(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.Number)
            {
                return value.GetInt64();
            }
            return null;
        }

        private static int? GetIntOrNull(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.Number)
            {
                return value.GetInt32();
            }
            return null;
        }

        public record StockSecurity
        {
            // Описание
            public string? SECID { get; init; }
            public string? SHORTNAME { get; init; }
            public string? LATNAME { get; init; }
            public string? SECNAME { get; init; }
            public string? ISIN { get; init; }
            public string? CURRENCYID { get; init; }
            public string? REGNUMBER { get; init; }

            //Параметры
            public string? BOARDID { get; init; }
            
            public int? LOTSIZE { get; init; }
            public decimal? FACEVALUE { get; init; }
            public string? STATUS { get; init; }
            public string? BOARDNAME { get; init; }
            public decimal? MINSTEP { get; init; }
            public int? LISTLEVEL { get; init; }
            public long? ISSUESIZE { get; init; }


            // Вчерашние данные
            public decimal? PREVPRICE { get; init; }           
            public decimal? PREVWAPRICE { get; init; }         
            public decimal? PREVLEGALCLOSEPRICE { get; init; }
            public string? PREVDATE { get; init; }            

            
            //Расчёты
            public string? SETTLEDATE { get; init; }
        }

    }
}
