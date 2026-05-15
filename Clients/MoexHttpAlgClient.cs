using History_DataMoex.Contracts.Dto;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Contracts.Pagination;
using History_DataMoex.Options;
using History_DataMoex.Parsing;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

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

        public async Task<string> GetRaw(
            string method,
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            string requestUrl = _options.BaseUrl + method;
            queryParams ??= new Dictionary<string, string>();
            if (queryParams.Count > 0)
            {
                QueryString queryString = QueryString.Create(queryParams);
                requestUrl += queryString.ToString();
            }
            EnsureApiKeyConfigured();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.Key}");
            var response = await _httpClient.SendAsync(request, cancellationToken);
            // Не бросаем — видим что MOEX ответил
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public async IAsyncEnumerable<List<CandlesDTO>> GetCandles(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            int queryStart = 0;
            queryParams ??= new Dictionary<string, string>();

            if (queryParams.TryGetValue("start", out string? start) && int.TryParse(start, out int parseValue)) 
            {
                queryStart = parseValue;
            }
            
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                //using JsonDocument jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);


                //List<CandlesDTO> candlesList = ParsingALG.ParseAlgCandles(jsonDocument);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                List<CandlesDTO> candlesList = ParsingAlgUtf8.ParseAlgCandles(bytes);
                yield return candlesList;
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

            
        }

        

        public async IAsyncEnumerable<List<Hi2AssetDTO>> GetHi2Asset5m(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {

            queryParams ??= new Dictionary<string, string>();

            

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<Hi2AssetDTO> hi2Assets = ParsingAlgUtf8.ParseHi2Stock(bytes, out PaginationCursorDTO cursor);
                yield return hi2Assets;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();

            }


            
        }

        public async IAsyncEnumerable<List<Hi2FuturesDTO>> GetHi2Furures5m(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {

            queryParams ??= new Dictionary<string, string>();

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<Hi2FuturesDTO> hi2Futures = ParsingAlgUtf8.ParseHi2Futures(bytes, out PaginationCursorDTO cursor);
                yield return hi2Futures;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();

            }
        }

        public async IAsyncEnumerable<List<MegaAlertsAssetsDTO>> GetMegaAlerts(
            string metod,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            queryParams ??= new Dictionary<string, string>();

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(metod, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<MegaAlertsAssetsDTO> megaAlerts = ParsingAlgUtf8.ParseMegaAlertsStock(bytes, out PaginationCursorDTO cursor);
                yield return megaAlerts;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();
            }
        }

        public async IAsyncEnumerable<List<MegaAlertsFuturesDTO>> GetMegaAlertsFutures(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            queryParams ??= new Dictionary<string, string>();

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<MegaAlertsFuturesDTO> megaAlertsFutures = ParsingAlgUtf8.ParseMegaAlertsFutures(bytes, out PaginationCursorDTO cursor);
                yield return megaAlertsFutures;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();
            }
        }

        public async IAsyncEnumerable<List<SuperCandlesTradeStats5mDTO>> GetSuperCandlesTradeStats5m(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            
            queryParams ??= new Dictionary<string, string>();
            
            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<SuperCandlesTradeStats5mDTO> tradeStats = ParsingAlgUtf8.ParseTradeStatsStock(bytes, out PaginationCursorDTO cursor);
                yield return tradeStats;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();
                
            }
        }

        public async IAsyncEnumerable<List<SuperCandlesOrderBookStats5mDTO>> GetSuperCandlesOrderBookStats5m(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {

            queryParams ??= new Dictionary<string, string>();

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<SuperCandlesOrderBookStats5mDTO> orderBookStats = ParsingAlgUtf8.ParseOBStatsStock(bytes, out PaginationCursorDTO cursor);
                yield return orderBookStats;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();

            }
        }

        public async IAsyncEnumerable<List<SuperCandlesOrderStats5mDTO>> GetSuperCandlesOrderStats5m(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {

            queryParams ??= new Dictionary<string, string>();

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<SuperCandlesOrderStats5mDTO> orderStats = ParsingAlgUtf8.ParseOrderStatsStock(bytes, out PaginationCursorDTO cursor);
                yield return orderStats;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();

            }
        }

        public async IAsyncEnumerable<List<SuperCandlesFuturesOrderBookStats5mDTO>> GetSuperCandlesFuturesOrderBookStats5m(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            queryParams ??= new Dictionary<string, string>();

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<SuperCandlesFuturesOrderBookStats5mDTO> orderBookStats = ParsingAlgUtf8.ParseOBStatsFutures(bytes, out PaginationCursorDTO cursor);
                yield return orderBookStats;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();
            }
        }

        public async Task<List<FutoiDTO>> GetFutoi(
            string method,
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
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
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<FutoiDTO> page = ParsingAlgUtf8.ParseFutoi(bytes);
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

        public async IAsyncEnumerable<List<FutoiDTO>> StreamFutoi(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            int queryStart = 0;
            queryParams ??= new Dictionary<string, string>();

            if (queryParams.TryGetValue("start", out string? start) && int.TryParse(start, out int parseValue))
            {
                queryStart = parseValue;
            }
           
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<FutoiDTO> page = ParsingAlgUtf8.ParseFutoi(bytes);
                yield return page;
                if (page.Count >= 1000)
                {
                    queryStart += 1000;
                    queryParams["offset"] = queryStart.ToString();
                }
                else
                {
                    break;
                }
            }
            //return all;
        }

        public async IAsyncEnumerable<List<SuperCandlesFuturesTradeStats5mDTO>> GetSuperCandlesFuturesTradeStats5m(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            queryParams ??= new Dictionary<string, string>();

            
            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                List<SuperCandlesFuturesTradeStats5mDTO> tradeStats = ParsingAlgUtf8.ParseTradeStatsFutures(bytes, out PaginationCursorDTO cursor);
                yield return tradeStats;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();

            }
        }


        private async Task<HttpResponseMessage> SendRequestAsync(string method, Dictionary<string, string>? queryParams = null, CancellationToken cancellationToken = default)
        {
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;
            queryParams ??= new Dictionary<string, string>();
            if (queryParams.Count > 0)
            {
                QueryString queryString = QueryString.Create(queryParams!);
                requestUrl += queryString.ToString();
            }
            EnsureApiKeyConfigured();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.Key}");
            var response = await _httpClient.SendAsync(request,HttpCompletionOption.ResponseHeadersRead ,cancellationToken);
            response.EnsureSuccessStatusCode();
            return response;
        }




        private void EnsureApiKeyConfigured()
        {
            if (string.IsNullOrWhiteSpace(_options.Key))
            {
                throw new InvalidOperationException(
                    "MOEX ALGOPACK API key is not configured. Set MoexAlg:Key via user-secrets or environment variable.");
            }
        }
    }
}
