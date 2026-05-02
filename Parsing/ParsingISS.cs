using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public static class ParsingISS
    {

        public static List<StockSecurityDTO> ParseIssSecurityStock(JsonDocument jsonDocument)
        {
            List<StockSecurityDTO> stockSecurities = new List<StockSecurityDTO>();

            JsonElement root = jsonDocument.RootElement;
            JsonElement securities = root.GetProperty("securities");
            JsonElement columns = securities.GetProperty("columns");
            const int arraysize = 9;
            int[] columnIndexs = new int[arraysize];

            string [] columnNames = new string[arraysize] { "SECID", "SHORTNAME", "SECNAME", "BOARDID", "PREVLEGALCLOSEPRICE", "LOTSIZE", "FACEVALUE", "MARKETCODE", "PREVDATE" };
            for (int i = 0; i < arraysize; i++)
            {
               for(int j = 0;j<columns.GetArrayLength();j++)
                {
                    if (columns[j].GetString() == columnNames[i])
                    {
                        columnIndexs[i] = j;
                        break;
                    }
                }
            }

            JsonElement datas = securities.GetProperty("data");
            for(int i =0; i < datas.GetArrayLength();i++)
            {
                
                StockSecurityDTO stockSecurityDTO = new StockSecurityDTO
                {
                    SECID = GetStringOrNull(datas[i][columnIndexs[0]]),
                    SHORTNAME = GetStringOrNull(datas[i][columnIndexs[1]]),
                    SECNAME = GetStringOrNull(datas[i][columnIndexs[2]]),
                    BOARDID = GetStringOrNull(datas[i][columnIndexs[3]]),
                    PREVLEGALCLOSEPRICE = GetDecimalOrNull(datas[i][columnIndexs[4]]),
                    LOTSIZE = GetIntOrNull(datas[i][columnIndexs[5]]),
                    FACEVALUE = GetDoubleOrNull(datas[i][columnIndexs[6]]),
                    MARKETCODE = GetStringOrNull(datas[i][columnIndexs[7]]),
                    PREVDATE = GetDateTimeOrNull(datas[i][columnIndexs[8]])
                };
                stockSecurities.Add(stockSecurityDTO);
            }

            return stockSecurities;

        }

        private static string? GetStringOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
            return null;
        }
        private static decimal? GetDecimalOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDecimal();
            }
            return null;
        }
        private static double? GetDoubleOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDouble();
            }
            return null;
        }

        private static long? GetLongOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetInt64();
            }
            return null;
        }

        private static int? GetIntOrNull(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetInt32();
            }
            return null;
        }

        private static DateTime? GetDateTimeOrNull(JsonElement element)
        {
            if (element.TryGetDateTime(out DateTime dateTime) && element.ValueKind == JsonValueKind.String)
            {
                return dateTime;
            }
            return null;
        }

    }
}
