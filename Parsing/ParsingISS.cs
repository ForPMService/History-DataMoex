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
                    SECID = ParseHelpers.GetStringOrNull(datas[i][columnIndces[0]]),
                    SHORTNAME = ParseHelpers.GetStringOrNull(datas[i][columnIndces[1]]),
                    SECNAME = ParseHelpers.GetStringOrNull(datas[i][columnIndces[2]]),
                    BOARDID = ParseHelpers.GetStringOrNull(datas[i][columnIndces[3]]),
                    PREVLEGALCLOSEPRICE = ParseHelpers.GetDecimalOrNull(datas[i][columnIndces[4]]),
                    LOTSIZE = ParseHelpers.GetIntOrNull(datas[i][columnIndces[5]]),
                    FACEVALUE = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[6]]),
                    MARKETCODE = ParseHelpers.GetStringOrNull(datas[i][columnIndces[7]]),
                    PREVDATE = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndces[8]])
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
                    SECID = ParseHelpers.GetStringOrNull(datas[i][columnIndces[0]]),
                    SHORTNAME = ParseHelpers.GetStringOrNull(datas[i][columnIndces[1]]),
                    SECNAME = ParseHelpers.GetStringOrNull(datas[i][columnIndces[2]]),
                    ASSETCODE = ParseHelpers.GetStringOrNull(datas[i][columnIndces[3]]),
                    INITIALMARGIN = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[4]]),
                    PREVSETTLEPRICE = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[5]]),
                    PREVPRICE = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[6]]),
                    MINSTEP = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[7]]),
                    STEPPRICE = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[8]]),
                    LOTVOLUME = ParseHelpers.GetIntOrNull(datas[i][columnIndces[9]]),
                    LASTTRADEDATE = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndces[10]]),
                    LASTDELDATE = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndces[11]]),
                    PREVOPENPOSITION = ParseHelpers.GetLongOrNull(datas[i][columnIndces[12]]),
                    HIGHLIMIT = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[13]]),
                    LOWLIMIT = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[14]]),
                    DECIMALS = ParseHelpers.GetIntOrNull(datas[i][columnIndces[15]])
                };
                futuresSecurities.Add(futuresSecurityDTO);
            }

            return futuresSecurities;

        }

    }
}
