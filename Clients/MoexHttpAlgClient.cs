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
