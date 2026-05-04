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
            Span<int> columnIndices = stackalloc int[arraysize];

            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {

                if (columns[i].ValueEquals("open"u8))
                {
                    columnIndices[0] = i;
                    found++;
                    continue;
                }

                else if (columns[i].ValueEquals("close"u8))
                {
                    columnIndices[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("high"u8))
                {

                    columnIndices[2] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("low"u8))
                {
                    columnIndices[3] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("value"u8))
                {
                    columnIndices[4] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("volume"u8))
                {
                    columnIndices[5] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("begin"u8))
                {
                    columnIndices[6] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("end"u8))
                {
                    columnIndices[7] = i;
                    found++;
                    continue;
                }
                
            }


            JsonElement datas = candles.GetProperty("data");
            for (int i = 0; i < datas.GetArrayLength(); i++)
            {

                CandlesDTO candlesDTO = new CandlesDTO()
                {
                    Open = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[0]]),
                    Close = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[1]]),
                    High = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[2]]),
                    Low = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[3]]),
                    Value = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[4]]),
                    Volume = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[5]]),
                    Begin = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndices[6]]),
                    End = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndices[7]])
                };
                candlesList.Add(candlesDTO);
            }

            return candlesList;

        }

        public static List<SuperCandlesTradeStats5mDTO> ParseAlgCandlesTradeStat(JsonDocument jsonDocument)
        {
            List<SuperCandlesTradeStats5mDTO> tradeStatList = new List<SuperCandlesTradeStats5mDTO>();

            JsonElement root = jsonDocument.RootElement;
            JsonElement data = root.GetProperty("data");
            JsonElement columns = data.GetProperty("columns");
            const int arraysize = 27;
            Span<int> columnIndices = stackalloc int[arraysize];
            
            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {

                if (columns[i].ValueEquals("tradedate"u8))
                {
                    columnIndices[0] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("tradetime"u8))
                {
                    columnIndices[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("secid"u8))
                {
                    columnIndices[2] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_open"u8))
                {
                    columnIndices[3] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_high"u8))
                {
                    columnIndices[4] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_low"u8))
                {
                    columnIndices[5] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_close"u8))
                {
                    columnIndices[6] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_std"u8))
                {
                    columnIndices[7] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vol"u8))
                {
                    columnIndices[8] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("val"u8))
                {
                    columnIndices[9] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("trades"u8))
                {
                    columnIndices[10] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_vwap"u8))
                {
                    columnIndices[11] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_change"u8))
                {
                    columnIndices[12] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("trades_b"u8))
                {
                    columnIndices[13] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("trades_s"u8))
                {
                    columnIndices[14] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("val_b"u8))
                {
                    columnIndices[15] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("val_s"u8))
                {
                    columnIndices[16] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vol_b"u8))
                {
                    columnIndices[17] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vol_s"u8))
                {
                    columnIndices[18] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("disb"u8))
                {
                    columnIndices[19] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_vwap_b"u8))
                {
                    columnIndices[20] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("pr_vwap_s"u8))
                {
                    columnIndices[21] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("SYSTIME"u8))
                {
                    columnIndices[22] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("sec_pr_open"u8))
                {
                    columnIndices[23] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("sec_pr_high"u8))
                {
                    columnIndices[24] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("sec_pr_low"u8))
                {
                    columnIndices[25] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("sec_pr_close"u8))
                {
                    columnIndices[26] = i;
                    found++;
                    continue;
                }

            }


            JsonElement datas = data.GetProperty("data");
            for (int i = 0; i < datas.GetArrayLength(); i++)
            {

                SuperCandlesTradeStats5mDTO tradeStatsDTO = new SuperCandlesTradeStats5mDTO()
                {
                    TradeDate = ParseHelpers.GetStringOrNull(datas[i][columnIndices[0]]),
                    TradeTime = ParseHelpers.GetStringOrNull(datas[i][columnIndices[1]]),
                    SecId = ParseHelpers.GetStringOrNull(datas[i][columnIndices[2]]),

                    PrOpen = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[3]]),
                    PrHigh = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[4]]),
                    PrLow = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[5]]),
                    PrClose = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[6]]),

                    PrStd = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[7]]),

                    Vol = ParseHelpers.GetIntOrNull(datas[i][columnIndices[8]]),
                    Val = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[9]]),
                    Trades = ParseHelpers.GetIntOrNull(datas[i][columnIndices[10]]),

                    PrVwap = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[11]]),
                    PrChange = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[12]]),

                    TradesB = ParseHelpers.GetIntOrNull(datas[i][columnIndices[13]]),
                    TradesS = ParseHelpers.GetIntOrNull(datas[i][columnIndices[14]]),

                    ValB = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[15]]),
                    ValS = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[16]]),

                    VolB = ParseHelpers.GetLongOrNull(datas[i][columnIndices[17]]),
                    VolS = ParseHelpers.GetLongOrNull(datas[i][columnIndices[18]]),

                    Disb = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[19]]),

                    PrVwapB = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[20]]),
                    PrVwapS = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[21]]),

                    SysTime = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndices[22]]),

                    SecPrOpen = ParseHelpers.GetIntOrNull(datas[i][columnIndices[23]]),
                    SecPrHigh = ParseHelpers.GetIntOrNull(datas[i][columnIndices[24]]),
                    SecPrLow = ParseHelpers.GetIntOrNull(datas[i][columnIndices[25]]),
                    SecPrClose = ParseHelpers.GetIntOrNull(datas[i][columnIndices[26]])
                };
                tradeStatList.Add(tradeStatsDTO);
            }

            return tradeStatList;

        }

        public static List<SuperCandlesOrderBookStats5mDTO> ParseAlgOrderBookStats5m(JsonDocument jsonDocument)
        {
            List<SuperCandlesOrderBookStats5mDTO> orderBookStatsList = new List<SuperCandlesOrderBookStats5mDTO>();

            JsonElement root = jsonDocument.RootElement;
            JsonElement data = root.GetProperty("data");
            JsonElement columns = data.GetProperty("columns");

            const int arraysize = 21;
            Span<int> columnIndices = stackalloc int[arraysize];

            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("tradedate"u8))
                {
                    columnIndices[0] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("tradetime"u8))
                {
                    columnIndices[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("secid"u8))
                {
                    columnIndices[2] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("spread_bbo"u8))
                {
                    columnIndices[3] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("spread_lv10"u8))
                {
                    columnIndices[4] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("spread_1mio"u8))
                {
                    columnIndices[5] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("levels_b"u8))
                {
                    columnIndices[6] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("levels_s"u8))
                {
                    columnIndices[7] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vol_b"u8))
                {
                    columnIndices[8] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vol_s"u8))
                {
                    columnIndices[9] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("val_b"u8))
                {
                    columnIndices[10] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("val_s"u8))
                {
                    columnIndices[11] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("imbalance_vol_bbo"u8))
                {
                    columnIndices[12] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("imbalance_val_bbo"u8))
                {
                    columnIndices[13] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("imbalance_vol"u8))
                {
                    columnIndices[14] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("imbalance_val"u8))
                {
                    columnIndices[15] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vwap_b"u8))
                {
                    columnIndices[16] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vwap_s"u8))
                {
                    columnIndices[17] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vwap_b_1mio"u8))
                {
                    columnIndices[18] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("vwap_s_1mio"u8))
                {
                    columnIndices[19] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("SYSTIME"u8))
                {
                    columnIndices[20] = i;
                    found++;
                    continue;
                }
            }

            JsonElement datas = data.GetProperty("data");

            for (int i = 0; i < datas.GetArrayLength(); i++)
            {
                SuperCandlesOrderBookStats5mDTO orderBookStatsDTO = new SuperCandlesOrderBookStats5mDTO()
                {
                    TradeDate = ParseHelpers.GetStringOrNull(datas[i][columnIndices[0]]),
                    TradeTime = ParseHelpers.GetStringOrNull(datas[i][columnIndices[1]]),
                    SecId = ParseHelpers.GetStringOrNull(datas[i][columnIndices[2]]),

                    SpreadBbo = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[3]]),
                    SpreadLv10 = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[4]]),
                    Spread1Mio = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[5]]),

                    LevelsB = ParseHelpers.GetIntOrNull(datas[i][columnIndices[6]]),
                    LevelsS = ParseHelpers.GetIntOrNull(datas[i][columnIndices[7]]),

                    VolB = ParseHelpers.GetLongOrNull(datas[i][columnIndices[8]]),
                    VolS = ParseHelpers.GetLongOrNull(datas[i][columnIndices[9]]),
                    ValB = ParseHelpers.GetLongOrNull(datas[i][columnIndices[10]]),
                    ValS = ParseHelpers.GetLongOrNull(datas[i][columnIndices[11]]),

                    ImbalanceVolBbo = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[12]]),
                    ImbalanceValBbo = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[13]]),
                    ImbalanceVol = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[14]]),
                    ImbalanceVal = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[15]]),

                    VwapB = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[16]]),
                    VwapS = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[17]]),
                    VwapB1Mio = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[18]]),
                    VwapS1Mio = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[19]]),

                    SysTime = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndices[20]])
                };

                orderBookStatsList.Add(orderBookStatsDTO);
            }

            return orderBookStatsList;
        }

        public static List<SuperCandlesOrderStats5mDTO> ParseAlgOrderStats5m(JsonDocument jsonDocument)
        {
            List<SuperCandlesOrderStats5mDTO> orderStatsList = new List<SuperCandlesOrderStats5mDTO>();

            JsonElement root = jsonDocument.RootElement;
            JsonElement data = root.GetProperty("data");
            JsonElement columns = data.GetProperty("columns");

            const int arraysize = 26;
            Span<int> columnIndices = stackalloc int[arraysize];

            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {
                if (columns[i].ValueEquals("tradedate"u8)) { columnIndices[0] = i; found++; continue; }
                else if (columns[i].ValueEquals("tradetime"u8)) { columnIndices[1] = i; found++; continue; }
                else if (columns[i].ValueEquals("secid"u8)) { columnIndices[2] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_orders_b"u8)) { columnIndices[3] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_orders_s"u8)) { columnIndices[4] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_val_b"u8)) { columnIndices[5] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_val_s"u8)) { columnIndices[6] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_vol_b"u8)) { columnIndices[7] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_vol_s"u8)) { columnIndices[8] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_vwap_b"u8)) { columnIndices[9] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_vwap_s"u8)) { columnIndices[10] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_vol"u8)) { columnIndices[11] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_val"u8)) { columnIndices[12] = i; found++; continue; }
                else if (columns[i].ValueEquals("put_orders"u8)) { columnIndices[13] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_orders_b"u8)) { columnIndices[14] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_orders_s"u8)) { columnIndices[15] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_val_b"u8)) { columnIndices[16] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_val_s"u8)) { columnIndices[17] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_vol_b"u8)) { columnIndices[18] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_vol_s"u8)) { columnIndices[19] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_vwap_b"u8)) { columnIndices[20] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_vwap_s"u8)) { columnIndices[21] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_vol"u8)) { columnIndices[22] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_val"u8)) { columnIndices[23] = i; found++; continue; }
                else if (columns[i].ValueEquals("cancel_orders"u8)) { columnIndices[24] = i; found++; continue; }
                else if (columns[i].ValueEquals("SYSTIME"u8)) { columnIndices[25] = i; found++; continue; }
            }

            JsonElement datas = data.GetProperty("data");

            for (int i = 0; i < datas.GetArrayLength(); i++)
            {
                SuperCandlesOrderStats5mDTO orderStatsDTO = new SuperCandlesOrderStats5mDTO()
                {
                    TradeDate = ParseHelpers.GetStringOrNull(datas[i][columnIndices[0]]),
                    TradeTime = ParseHelpers.GetStringOrNull(datas[i][columnIndices[1]]),
                    SecId = ParseHelpers.GetStringOrNull(datas[i][columnIndices[2]]),

                    PutOrdersB = ParseHelpers.GetIntOrNull(datas[i][columnIndices[3]]),
                    PutOrdersS = ParseHelpers.GetIntOrNull(datas[i][columnIndices[4]]),
                    PutValB = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[5]]),
                    PutValS = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[6]]),
                    PutVolB = ParseHelpers.GetIntOrNull(datas[i][columnIndices[7]]),
                    PutVolS = ParseHelpers.GetIntOrNull(datas[i][columnIndices[8]]),
                    PutVwapB = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[9]]),
                    PutVwapS = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[10]]),
                    PutVol = ParseHelpers.GetIntOrNull(datas[i][columnIndices[11]]),
                    PutVal = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[12]]),
                    PutOrders = ParseHelpers.GetIntOrNull(datas[i][columnIndices[13]]),

                    CancelOrdersB = ParseHelpers.GetIntOrNull(datas[i][columnIndices[14]]),
                    CancelOrdersS = ParseHelpers.GetIntOrNull(datas[i][columnIndices[15]]),
                    CancelValB = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[16]]),
                    CancelValS = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[17]]),
                    CancelVolB = ParseHelpers.GetIntOrNull(datas[i][columnIndices[18]]),
                    CancelVolS = ParseHelpers.GetLongOrNull(datas[i][columnIndices[19]]),
                    CancelVwapB = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[20]]),
                    CancelVwapS = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[21]]),
                    CancelVol = ParseHelpers.GetLongOrNull(datas[i][columnIndices[22]]),
                    CancelVal = ParseHelpers.GetDoubleOrNull(datas[i][columnIndices[23]]),
                    CancelOrders = ParseHelpers.GetLongOrNull(datas[i][columnIndices[24]]),

                    SysTime = ParseHelpers.GetDateTimeOrNull(datas[i][columnIndices[25]])
                };

                orderStatsList.Add(orderStatsDTO);
            }

            return orderStatsList;
        }

        public static PaginationCursorDTO ParseAlgCandlesDataCursor(JsonDocument jsonDocument)
        {
            

            JsonElement root = jsonDocument.RootElement;
            JsonElement data = root.GetProperty("data.cursor");
            JsonElement columns = data.GetProperty("columns");
            const int arraysize = 3;
            Span<int> columnIndices = stackalloc int[arraysize];

            int found = 0;

            for (int i = 0; i < columns.GetArrayLength() && found < arraysize; i++)
            {

                if (columns[i].ValueEquals("INDEX"u8))
                {
                    columnIndices[0] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("TOTAL"u8))
                {
                    columnIndices[1] = i;
                    found++;
                    continue;
                }
                else if (columns[i].ValueEquals("PAGESIZE"u8))
                {
                    columnIndices[2] = i;
                    found++;
                    continue;
                }
                

            }


            JsonElement datas = data.GetProperty("data");
           

            PaginationCursorDTO paginationCursor = new PaginationCursorDTO()
            {
                Index = ParseHelpers.GetIntOrNull(datas[0][columnIndices[0]]),
                Total = ParseHelpers.GetIntOrNull(datas[0][columnIndices[1]]),
                PageSize = ParseHelpers.GetIntOrNull(datas[0][columnIndices[2]])
            };
                
            

            return paginationCursor;

        }
    }
}
