using History_DataMoex.Clients.Errors;
using History_DataMoex.Contracts.Dto;
using History_DataMoex.Contracts.Dto.Algopack;
using History_DataMoex.Contracts.Pagination;
using History_DataMoex.Infrastructure.Buffers;
using History_DataMoex.Options;
using History_DataMoex.Parsing;
using Microsoft.Extensions.Options;
using Polly.Timeout;
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);
                List<CandlesDTO> candlesList = ParsingAlgUtf8.ParseAlgCandles(rentedArr.Span);
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<Hi2AssetDTO> hi2Assets = ParsingAlgUtf8.ParseHi2Stock(rentedArr.Span, out PaginationCursorDTO cursor);
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<Hi2FuturesDTO> hi2Futures = ParsingAlgUtf8.ParseHi2Futures(rentedArr.Span, out PaginationCursorDTO cursor);
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<MegaAlertsAssetsDTO> megaAlerts = ParsingAlgUtf8.ParseMegaAlertsStock(rentedArr.Span, out PaginationCursorDTO cursor);
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<MegaAlertsFuturesDTO> megaAlertsFutures = ParsingAlgUtf8.ParseMegaAlertsFutures(rentedArr.Span, out PaginationCursorDTO cursor);
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<SuperCandlesTradeStats5mDTO> tradeStats = ParsingAlgUtf8.ParseTradeStatsStock(rentedArr.Span, out PaginationCursorDTO cursor);
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<SuperCandlesOrderBookStats5mDTO> orderBookStats = ParsingAlgUtf8.ParseOBStatsStock(rentedArr.Span, out PaginationCursorDTO cursor);
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<SuperCandlesOrderStats5mDTO> orderStats = ParsingAlgUtf8.ParseOrderStatsStock(rentedArr.Span, out PaginationCursorDTO cursor);
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<SuperCandlesFuturesOrderBookStats5mDTO> orderBookStats = ParsingAlgUtf8.ParseOBStatsFutures(rentedArr.Span, out PaginationCursorDTO cursor);
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

        public async IAsyncEnumerable<List<FutoiDTO>> StreamFutoi(
            string method,
            Dictionary<string, string>? queryParams = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            queryParams ??= new Dictionary<string, string>();

            // FUTOI API не поддерживает пагинацию (start игнорируется, лимит 1000 строк).
            // Разбиваем диапазон дат по одному дню — один день Si ≈ 470 строк, всегда < 1000.

            if (!queryParams.TryGetValue("from", out string? fromStr)
                || !queryParams.TryGetValue("till", out string? tillStr))
            {
                throw new InvalidOperationException(
                    "StreamFutoi requires 'from' and 'till' in queryParams.");
            }

            if (!DateTime.TryParseExact(fromStr, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime fromDate)
                || !DateTime.TryParseExact(tillStr, "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime tillDate))
            {
                throw new InvalidOperationException(
                    $"StreamFutoi: invalid date format. from='{fromStr}', till='{tillStr}'. Expected yyyy-MM-dd.");
            }

            // Убрать start/offset если были — MOEX их игнорирует
            queryParams.Remove("start");
            queryParams.Remove("offset");

            for (DateTime date = fromDate; date <= tillDate; date = date.AddDays(1))
            {
                cancellationToken.ThrowIfCancellationRequested();

                queryParams["from"] = date.ToString("yyyy-MM-dd");
                queryParams["till"] = date.ToString("yyyy-MM-dd");

                using var response = await SendRequestAsync(method, queryParams, cancellationToken);
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<FutoiDTO> page = ParsingAlgUtf8.ParseFutoi(rentedArr.Span);

                if (page.Count > 0)
                {
                    yield return page;
                }
            }
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
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<SuperCandlesFuturesTradeStats5mDTO> tradeStats = ParsingAlgUtf8.ParseTradeStatsFutures(rentedArr.Span, out PaginationCursorDTO cursor);
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
            try
            {
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                HttpClientHelpers.EnsureSuccessOrThrow(response, method);
                return response;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                throw new MoexTimeoutException($"MOEX request timeout for {method}", method, "http_client", _options.RequestTimeout, ex);
            }
            catch (TimeoutRejectedException ex)
            {
                throw new MoexTimeoutException($"MOEX attempt timeout for {method}", method, "polly_attempt", null, ex);
            }
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
