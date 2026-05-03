using History_DataMoex.DataTransfers;
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
            Span<int> columnIndces = stackalloc int[arraysize];

            //string [] columnNames = new string[arraysize] { "SECID", "SHORTNAME", "SECNAME", "BOARDID", "PREVLEGALCLOSEPRICE", "LOTSIZE", "FACEVALUE", "MARKETCODE", "PREVDATE" };
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {

                if (columns[i].ValueEquals("SECID"u8))
                {
                    columnIndces[0] = i;
                    found++;
                    continue;
                }

                else if (columns[i].ValueEquals("SHORTNAME"u8))
                {
                    columnIndces[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("SECNAME"u8))
                {
                    
                    columnIndces[2] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("BOARDID"u8))
                { 
                    columnIndces[3] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVLEGALCLOSEPRICE"u8))
                {
                    columnIndces[4] = i;
                        found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LOTSIZE"u8))    
                {
                    columnIndces[5] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("FACEVALUE"u8))
                {
                    columnIndces[6] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("MARKETCODE"u8))
                {
                    columnIndces[7] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVDATE"u8))
                {
                    columnIndces[8] = i;
                    found++;
                    continue;
                }

            }

           
            JsonElement datas = securities.GetProperty("data");
            for(int i =0; i < datas.GetArrayLength();i++)
            {
                
                StockSecurityDTO stockSecurityDTO = new StockSecurityDTO
                {
                    SECID = GetStringOrNull(datas[i][columnIndces[0]]),
                    SHORTNAME = GetStringOrNull(datas[i][columnIndces[1]]),
                    SECNAME = GetStringOrNull(datas[i][columnIndces[2]]),
                    BOARDID = GetStringOrNull(datas[i][columnIndces[3]]),
                    PREVLEGALCLOSEPRICE = GetDecimalOrNull(datas[i][columnIndces[4]]),
                    LOTSIZE = GetIntOrNull(datas[i][columnIndces[5]]),
                    FACEVALUE = GetDoubleOrNull(datas[i][columnIndces[6]]),
                    MARKETCODE = GetStringOrNull(datas[i][columnIndces[7]]),
                    PREVDATE = GetDateTimeOrNull(datas[i][columnIndces[8]])
                };
                stockSecurities.Add(stockSecurityDTO);
            }

            return stockSecurities;

        }

        public static List<FuturesSecurityDTO> ParseIssSecurityFutures(JsonDocument jsonDocument)
        {
            List<FuturesSecurityDTO> futuresSecurities = new List<FuturesSecurityDTO>();

            JsonElement root = jsonDocument.RootElement;
            JsonElement securities = root.GetProperty("securities");
            JsonElement columns = securities.GetProperty("columns");
            const int arraysize = 16;
            Span<int> columnIndces = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength()&& found<arraysize; i++)
            {
                if (columns[i].ValueEquals("SECID"u8))
                {
                    columnIndces[0] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("SHORTNAME"u8))
                {
                    columnIndces[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("SECNAME"u8))
                {
                    columnIndces[2] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("ASSETCODE"u8))
                {
                    columnIndces[3] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("INITIALMARGIN"u8))
                {
                    columnIndces[4] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVSETTLEPRICE"u8))
                {
                    columnIndces[5] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVPRICE"u8))
                {
                    columnIndces[6] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("MINSTEP"u8))
                {
                    columnIndces[7] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("STEPPRICE"u8))
                {
                    columnIndces[8] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LOTVOLUME"u8))
                {
                    columnIndces[9] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LASTTRADEDATE"u8))
                {
                    columnIndces[10] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LASTDELDATE"u8))
                {
                    columnIndces[11] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVOPENPOSITION"u8))
                {
                    columnIndces[12] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("HIGHLIMIT"u8))
                {
                    columnIndces[13] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LOWLIMIT"u8))
                {
                    columnIndces[14] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("DECIMALS"u8))
                {
                    columnIndces[15] = i;
                    found++;
                    continue;
                }
            }

            JsonElement datas = securities.GetProperty("data");
            for (int i = 0; i < datas.GetArrayLength(); i++)
            {

                FuturesSecurityDTO futuresSecurityDTO = new FuturesSecurityDTO
                {
                    SECID = GetStringOrNull(datas[i][columnIndces[0]]),
                    SHORTNAME = GetStringOrNull(datas[i][columnIndces[1]]),
                    SECNAME = GetStringOrNull(datas[i][columnIndces[2]]),
                    ASSETCODE = GetStringOrNull(datas[i][columnIndces[3]]),
                    INITIALMARGIN = GetDoubleOrNull(datas[i][columnIndces[4]]),
                    PREVSETTLEPRICE = GetDoubleOrNull(datas[i][columnIndces[5]]),
                    PREVPRICE = GetDoubleOrNull(datas[i][columnIndces[6]]),
                    MINSTEP = GetDoubleOrNull(datas[i][columnIndces[7]]),
                    STEPPRICE = GetDoubleOrNull(datas[i][columnIndces[8]]),
                    LOTVOLUME = GetIntOrNull(datas[i][columnIndces[9]]),
                    LASTTRADEDATE = GetDateTimeOrNull(datas[i][columnIndces[10]]),
                    LASTDELDATE = GetDateTimeOrNull(datas[i][columnIndces[11]]),
                    PREVOPENPOSITION = GetLongOrNull(datas[i][columnIndces[12]]),
                    HIGHLIMIT = GetDoubleOrNull(datas[i][columnIndces[13]]),
                    LOWLIMIT = GetDoubleOrNull(datas[i][columnIndces[14]]),
                    DECIMALS = GetIntOrNull(datas[i][columnIndces[15]])
                };
                futuresSecurities.Add(futuresSecurityDTO);
            }

            return futuresSecurities;

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
