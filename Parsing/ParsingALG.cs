using History_DataMoex.DataTransfers;
using System.Text.Json;

namespace History_DataMoex.Parsing
{
    public static class ParsingALG
    {
        public static List<CandlesDTO> ParseAlgCandles(JsonDocument jsonDocument)
        {
            List<CandlesDTO> candlesList = new List<CandlesDTO>();

            JsonElement root = jsonDocument.RootElement;
            JsonElement candles = root.GetProperty("candles");
            JsonElement columns = candles.GetProperty("columns");
            const int arraysize = 8;
            Span<int> columnIndces = stackalloc int[arraysize];

            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {

                if (columns[i].ValueEquals("open"u8))
                {
                    columnIndces[0] = i;
                    found++;
                    continue;
                }

                else if (columns[i].ValueEquals("close"u8))
                {
                    columnIndces[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("high"u8))
                {

                    columnIndces[2] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("low"u8))
                {
                    columnIndces[3] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("value"u8))
                {
                    columnIndces[4] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("volume"u8))
                {
                    columnIndces[5] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("begin"u8))
                {
                    columnIndces[6] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("end"u8))
                {
                    columnIndces[7] = i;
                    found++;
                    continue;
                }
                
            }


            JsonElement datas = candles.GetProperty("data");
            for (int i = 0; i < datas.GetArrayLength(); i++)
            {

                CandlesDTO candlesDTO = new CandlesDTO()
                {
                    Open = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[0]]),
                    Close = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[1]]),
                    High = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[2]]),
                    Low = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[3]]),
                    Value = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[4]]),
                    Volume = ParseHelpers.GetDoubleOrNull(datas[i][columnIndces[5]]),
                    Begin = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndces[6]]),
                    End = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndces[7]])
                };
                candlesList.Add(candlesDTO);
            }

            return candlesList;

        }
        
    }
}
