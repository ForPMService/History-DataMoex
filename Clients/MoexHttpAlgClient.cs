using History_DataMoex.DataTransfers;
using History_DataMoex.Options;
using History_DataMoex.Parsing;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace History_DataMoex.Clients
{
    public class MoexHttpAlgClient
    {
        private readonly MoexAlgOptions _options;
        private readonly HttpClient _httpClient;

        public MoexHttpAlgClient(IOptions<MoexAlgOptions> options, HttpClient httpClient)
        {
            _options = options.Value;
            _httpClient = httpClient;
        }

        public async Task<string> GetRaw(string method, Dictionary<string, string>? queryParams = null)
        {
            string requestUrl = _options.BaseUrl + method;
            queryParams ??= new Dictionary<string, string>();
            if (queryParams.Count > 0)
            {
                QueryString queryString = QueryString.Create(queryParams);
                requestUrl += queryString.ToString();
            }
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.Key}");
            var response = await _httpClient.SendAsync(request);
            // Не бросаем — видим что MOEX ответил
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<CandlesDTO>> GetCandles(string method, Dictionary<string, string>? queryParams = null)
        {
            int queryStart = 0;
            queryParams ??= new Dictionary<string, string>();

            if (queryParams.TryGetValue("start", out string? start) && int.TryParse(start, out int parseValue)) 
            {
                queryStart = parseValue;
            }
            List<CandlesDTO> candles = new List<CandlesDTO>();
            while (true)
            {
                var response = await SendRequest(method, queryParams);
                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());


                List<CandlesDTO> candlesList = ParsingALG.ParseAlgCandles(jsonDocument);
                candles.AddRange(candlesList);
                if (candlesList.Count>=500)
                {
                    queryStart += 500;
                    queryParams["start"] = queryStart.ToString();
                }
                else
                {
                    break;
                }
                

            }

            return candles;
        }

        public async Task <List<Hi2AssetDTO>> GetHi2Asset5m(string method, Dictionary<string, string>? queryParams = null)
        {

            queryParams ??= new Dictionary<string, string>();

            List<Hi2AssetDTO> hi2AssetsAll = new List<Hi2AssetDTO>();

            while (true)
            {
                var response = await SendRequest(method, queryParams);
                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());


                List<Hi2AssetDTO> hi2Assets = ParsingALG.ParseHi2Assets(jsonDocument);
                PaginationCursorDTO dataCursoPag = ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);
                hi2AssetsAll.AddRange(hi2Assets);
                if (dataCursoPag.Index + dataCursoPag.PageSize >= dataCursoPag.Total)
                {
                    break;
                }
                queryParams!["start"] = (dataCursoPag.Index!.Value + dataCursoPag.PageSize!.Value).ToString();

            }


            return hi2AssetsAll;
        }

        public async Task<List<Hi2FuturesDTO>> GetHi2Furures5m(string method, Dictionary<string, string>? queryParams = null)
        {

            queryParams ??= new Dictionary<string, string>();

            List<Hi2FuturesDTO> hi2FuturesAll = new List<Hi2FuturesDTO>();

            while (true)
            {
                var response = await SendRequest(method, queryParams);
                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());


                List<Hi2FuturesDTO> hi2Futures = ParsingALG.ParseHi2Futures(jsonDocument);
                PaginationCursorDTO dataCursoPag = ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);
                hi2FuturesAll.AddRange(hi2Futures);
                if (dataCursoPag.Index + dataCursoPag.PageSize >= dataCursoPag.Total)
                {
                    break;
                }
                queryParams!["start"] = (dataCursoPag.Index!.Value + dataCursoPag.PageSize!.Value).ToString();

            }


            return hi2FuturesAll;
        }

        public async Task<List<MegaAlertsAssetsDTO>> GetMegaAlerts(string metod, Dictionary<string, string>? queryParams = null)
        {
            queryParams ??= new Dictionary<string, string>();

            List<MegaAlertsAssetsDTO> megaAlertsAll = new List<MegaAlertsAssetsDTO>();

            while (true)
            {
                var response = await SendRequest(metod, queryParams);

                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                List<MegaAlertsAssetsDTO> megaAlerts = ParsingALG.ParseMegaAlerts(jsonDocument);
                PaginationCursorDTO dataCursorPag = ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);

                megaAlertsAll.AddRange(megaAlerts);

                if (dataCursorPag.Index + dataCursorPag.PageSize >= dataCursorPag.Total)
                {
                    break;
                }

                queryParams["start"] =
                    (dataCursorPag.Index!.Value + dataCursorPag.PageSize!.Value).ToString();
            }

            return megaAlertsAll;
        }

        public async Task<List<MegaAlertsFuturesDTO>> GetMegaAlertsFutures(string method, Dictionary<string, string>? queryParams = null)
        {
            queryParams ??= new Dictionary<string, string>();

            List<MegaAlertsFuturesDTO> megaAlertsFuturesAll = new List<MegaAlertsFuturesDTO>();

            while (true)
            {
                var response = await SendRequest(method, queryParams);

                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                List<MegaAlertsFuturesDTO> megaAlertsFutures = ParsingALG.ParseMegaAlertsFutures(jsonDocument);

                PaginationCursorDTO dataCursorPag = ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);

                megaAlertsFuturesAll.AddRange(megaAlertsFutures);

                if (dataCursorPag.Index + dataCursorPag.PageSize >= dataCursorPag.Total)
                {
                    break;
                }

                queryParams["start"] = (dataCursorPag.Index!.Value + dataCursorPag.PageSize!.Value).ToString();
            }

            return megaAlertsFuturesAll;
        }

        public async Task<List<SuperCandlesTradeStats5mDTO>> GetSuperCandlesTradeStats5m(string method, Dictionary<string, string>? queryParams = null)
        {
            
            queryParams ??= new Dictionary<string, string>();

            List<SuperCandlesTradeStats5mDTO> tradeStatsall = new List<SuperCandlesTradeStats5mDTO>();
            
            while (true)
            {
                var response = await SendRequest(method, queryParams);
                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                
                List<SuperCandlesTradeStats5mDTO> tradeStats = ParsingALG.ParseAlgCandlesTradeStat(jsonDocument);
                PaginationCursorDTO dataCursoPag = ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);
                tradeStatsall.AddRange(tradeStats);
                if(dataCursoPag.Index + dataCursoPag.PageSize >= dataCursoPag.Total)
                {
                    break;
                }
                queryParams!["start"]= (dataCursoPag.Index!.Value + dataCursoPag.PageSize!.Value).ToString();
                
            }
            
            
            return tradeStatsall;
        }

        public async Task<List<SuperCandlesOrderBookStats5mDTO>> GetSuperCandlesOrderBookStats5m(string method, Dictionary<string, string>? queryParams = null)
        {

            queryParams ??= new Dictionary<string, string>();

            List<SuperCandlesOrderBookStats5mDTO> tradeStatsall = new List<SuperCandlesOrderBookStats5mDTO>();

            while (true)
            {
                var response = await SendRequest(method, queryParams);
                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());


                List<SuperCandlesOrderBookStats5mDTO> orderBookStats = ParsingALG.ParseAlgOrderBookStats5m(jsonDocument);
                PaginationCursorDTO dataCursoPag = ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);
                tradeStatsall.AddRange(orderBookStats);
                if (dataCursoPag.Index + dataCursoPag.PageSize >= dataCursoPag.Total)
                {
                    break;
                }
                queryParams!["start"] = (dataCursoPag.Index!.Value + dataCursoPag.PageSize!.Value).ToString();

            }


            return tradeStatsall;
        }

        public async Task<List<SuperCandlesOrderStats5mDTO>> GetSuperCandlesOrderStats5m(string method, Dictionary<string, string>? queryParams = null)
        {

            queryParams ??= new Dictionary<string, string>();

            List<SuperCandlesOrderStats5mDTO> tradeStatsall = new List<SuperCandlesOrderStats5mDTO>();

            while (true)
            {
                var response = await SendRequest(method, queryParams);
                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());


                List<SuperCandlesOrderStats5mDTO> orderStats = ParsingALG.ParseAlgOrderStats5m(jsonDocument);
                PaginationCursorDTO dataCursoPag = ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);
                tradeStatsall.AddRange(orderStats);
                if (dataCursoPag.Index + dataCursoPag.PageSize >= dataCursoPag.Total)
                {
                    break;
                }
                queryParams!["start"] = (dataCursoPag.Index!.Value + dataCursoPag.PageSize!.Value).ToString();

            }


            return tradeStatsall;
        }

        public async Task<List<SuperCandlesFuturesOrderBookStats5mDTO>> GetSuperCandlesFuturesOrderBookStats5m(string method, Dictionary<string, string>? queryParams = null)
        {
            queryParams ??= new Dictionary<string, string>();

            List<SuperCandlesFuturesOrderBookStats5mDTO> orderBookStatsAll =
                new List<SuperCandlesFuturesOrderBookStats5mDTO>();

            while (true)
            {
                var response = await SendRequest(method, queryParams);

                using JsonDocument jsonDocument =
                    JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                List<SuperCandlesFuturesOrderBookStats5mDTO> orderBookStats =
                    ParsingALG.ParseAlgFuturesOrderBook(jsonDocument);

                PaginationCursorDTO dataCursorPag =
                    ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);

                orderBookStatsAll.AddRange(orderBookStats);

                if (dataCursorPag.Index + dataCursorPag.PageSize >= dataCursorPag.Total)
                {
                    break;
                }

                queryParams["start"] =
                    (dataCursorPag.Index!.Value + dataCursorPag.PageSize!.Value).ToString();
            }

            return orderBookStatsAll;
        }

        public async Task<List<FutoiDTO>> GetFutoi(string method, Dictionary<string, string>? queryParams = null)
        {
            int queryStart = 0;
            queryParams ??= new Dictionary<string, string>();

            if (queryParams.TryGetValue("start", out string? start) && int.TryParse(start, out int parseValue))
            {
                queryStart = parseValue;
            }

            List<FutoiDTO> all = new List<FutoiDTO>();
            while (true)
            {
                var response = await SendRequest(method, queryParams);
                using JsonDocument jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                List<FutoiDTO> page = ParsingALG.ParseFutoi(jsonDocument);
                all.AddRange(page);
                if (page.Count >= 1000)
                {
                    queryStart += 1000;
                    queryParams["start"] = queryStart.ToString();
                }
                else
                {
                    break;
                }
            }
            return all;
        }

        public async Task<List<SuperCandlesFuturesTradeStats5mDTO>> GetSuperCandlesFuturesTradeStats5m(string method, Dictionary<string, string>? queryParams = null)
        {
            queryParams ??= new Dictionary<string, string>();

            List<SuperCandlesFuturesTradeStats5mDTO> tradeStatsAll =
                new List<SuperCandlesFuturesTradeStats5mDTO>();

            while (true)
            {
                var response = await SendRequest(method, queryParams);

                using JsonDocument jsonDocument =
                    JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                List<SuperCandlesFuturesTradeStats5mDTO> tradeStats =
                    ParsingALG.ParseFuturesTradeStats(jsonDocument);

                PaginationCursorDTO dataCursorPag =
                    ParsingALG.ParseAlgCandlesDataCursor(jsonDocument);

                tradeStatsAll.AddRange(tradeStats);

                if (dataCursorPag.Index + dataCursorPag.PageSize >= dataCursorPag.Total)
                {
                    break;
                }

                queryParams["start"] =
                    (dataCursorPag.Index!.Value + dataCursorPag.PageSize!.Value).ToString();
            }

            return tradeStatsAll;
        }

        private async Task<HttpResponseMessage> SendRequest(string method, Dictionary<string, string>? queryParams = null)
        {
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;
            queryParams ??= new Dictionary<string, string>();
            if (queryParams.Count > 0)
            {
                QueryString queryString = QueryString.Create(queryParams!);
                requestUrl += queryString.ToString();
            }
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.Key}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
