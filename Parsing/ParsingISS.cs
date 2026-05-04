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
            Span<int> columnIndices = stackalloc int[arraysize];

            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {

                if (columns[i].ValueEquals("SECID"u8))
                {
                    columnIndices[0] = i;
                    found++;
                    continue;
                }

                else if (columns[i].ValueEquals("SHORTNAME"u8))
                {
                    columnIndices[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("SECNAME"u8))
                {
                    
                    columnIndices[2] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("BOARDID"u8))
                { 
                    columnIndices[3] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVLEGALCLOSEPRICE"u8))
                {
                    columnIndices[4] = i;
                        found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LOTSIZE"u8))    
                {
                    columnIndices[5] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("FACEVALUE"u8))
                {
                    columnIndices[6] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("MARKETCODE"u8))
                {
                    columnIndices[7] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVDATE"u8))
                {
                    columnIndices[8] = i;
                    found++;
                    continue;
                }

            }

           
            JsonElement datas = securities.GetProperty("data");
            for(int i =0; i < datas.GetArrayLength();i++)
            {
                
                StockSecurityDTO stockSecurityDTO = new StockSecurityDTO
                {
                    SECID = ParseHelpers.GetStringOrNull(datas[i][columnIndices[0]]),
                    SHORTNAME = ParseHelpers.GetStringOrNull(datas[i][columnIndices[1]]),
                    SECNAME = ParseHelpers.GetStringOrNull(datas[i][columnIndices[2]]),
                    BOARDID = ParseHelpers.GetStringOrNull(datas[i][columnIndices[3]]),
                    PREVLEGALCLOSEPRICE = ParseHelpers.GetDecimalOrNull(datas[i][columnIndices[4]]),
                    LOTSIZE = ParseHelpers.GetIntOrNull(datas[i][columnIndices[5]]),
                    FACEVALUE = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[6]]),
                    MARKETCODE = ParseHelpers.GetStringOrNull(datas[i][columnIndices[7]]),
                    PREVDATE = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndices[8]])
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
            Span<int> columnIndices = stackalloc int[arraysize];
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength()&& found<arraysize; i++)
            {
                if (columns[i].ValueEquals("SECID"u8))
                {
                    columnIndices[0] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("SHORTNAME"u8))
                {
                    columnIndices[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("SECNAME"u8))
                {
                    columnIndices[2] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("ASSETCODE"u8))
                {
                    columnIndices[3] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("INITIALMARGIN"u8))
                {
                    columnIndices[4] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVSETTLEPRICE"u8))
                {
                    columnIndices[5] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVPRICE"u8))
                {
                    columnIndices[6] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("MINSTEP"u8))
                {
                    columnIndices[7] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("STEPPRICE"u8))
                {
                    columnIndices[8] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LOTVOLUME"u8))
                {
                    columnIndices[9] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LASTTRADEDATE"u8))
                {
                    columnIndices[10] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LASTDELDATE"u8))
                {
                    columnIndices[11] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PREVOPENPOSITION"u8))
                {
                    columnIndices[12] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("HIGHLIMIT"u8))
                {
                    columnIndices[13] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("LOWLIMIT"u8))
                {
                    columnIndices[14] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("DECIMALS"u8))
                {
                    columnIndices[15] = i;
                    found++;
                    continue;
                }
            }

            JsonElement datas = securities.GetProperty("data");
            for (int i = 0; i < datas.GetArrayLength(); i++)
            {

                FuturesSecurityDTO futuresSecurityDTO = new FuturesSecurityDTO
                {
                    SECID = ParseHelpers.GetStringOrNull(datas[i][columnIndices[0]]),
                    SHORTNAME = ParseHelpers.GetStringOrNull(datas[i][columnIndices[1]]),
                    SECNAME = ParseHelpers.GetStringOrNull(datas[i][columnIndices[2]]),
                    ASSETCODE = ParseHelpers.GetStringOrNull(datas[i][columnIndices[3]]),
                    INITIALMARGIN = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[4]]),
                    PREVSETTLEPRICE = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[5]]),
                    PREVPRICE = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[6]]),
                    MINSTEP = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[7]]),
                    STEPPRICE = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[8]]),
                    LOTVOLUME = ParseHelpers.GetIntOrNull(datas[i][columnIndices[9]]),
                    LASTTRADEDATE = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndices[10]]),
                    LASTDELDATE = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndices[11]]),
                    PREVOPENPOSITION = ParseHelpers.GetLongOrNull(datas[i][columnIndices[12]]),
                    HIGHLIMIT = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[13]]),
                    LOWLIMIT = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[14]]),
                    DECIMALS = ParseHelpers.GetIntOrNull(datas[i][columnIndices[15]])
                };
                futuresSecurities.Add(futuresSecurityDTO);
            }

            return futuresSecurities;

        }

    }
}
