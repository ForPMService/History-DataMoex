using History_DataMoex.Clients.Errors;
using History_DataMoex.Contracts.Dto;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Contracts.Pagination;
using History_DataMoex.Infrastructure.Buffers;
using History_DataMoex.Options;
using History_DataMoex.Parsing;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using System.Runtime.CompilerServices;

namespace History_DataMoex.Clients
{
    public class MoexHttpCalendarClient
    {
        private readonly MoexAlgOptions _options;
        private readonly HttpClient _httpClient;

        public MoexHttpCalendarClient(IOptions<MoexAlgOptions> options, HttpClient httpClient)
        {
            _options = options.Value;
            _httpClient = httpClient;
        }

        // ── Выходные дни ────────────────────────────────────────────

        public async Task<List<CalendarOffDaysAllDTO>> GetOffDaysAll(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars.json", cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            return ParsingCalendarUtf8.ParseOffDaysAll(rentedArr.Span);
        }

        public async Task<List<CalendarOffDaysMarketDTO>> GetStockOffDays(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock.json", cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            return ParsingCalendarUtf8.ParseOffDaysMarket(rentedArr.Span);
        }

        public async Task<List<CalendarOffDaysMarketDTO>> GetFuturesOffDays(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures.json", cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            return ParsingCalendarUtf8.ParseOffDaysMarket(rentedArr.Span);
        }

        // ── Сессии ────────────────────────────────────────────

        public async Task<(List<CalendarStockSessionDTO> Sessions, List<CalendarSessionTypeDTO> Types)>
            GetStockSessionWithTypes(CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/session.json", cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            return ParsingCalendarUtf8.ParseStockSession(rentedArr.Span);
        }

        public async Task<(List<CalendarFuturesSessionDTO> Sessions, List<CalendarSessionTypeDTO> Types)>
            GetFuturesSessionWithTypes(CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/session.json", cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            return ParsingCalendarUtf8.ParseFuturesSession(rentedArr.Span);
        }

        // ── B9.5: закомментированы — заменены на GetStockSessionWithTypes/GetFuturesSessionWithTypes/GetFuturesSecuritiesAll ──
        /*
        [Obsolete("Используйте GetStockSessionWithTypes() — один запрос вместо двух")]
        public async Task<List<CalendarStockSessionDTO>> GetStockSession(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/session.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarStockSession(doc);
        }

        [Obsolete("Используйте GetStockSessionWithTypes() — один запрос вместо двух")]
        public async Task<List<CalendarSessionTypeDTO>> GetStockSessionTypes(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/session.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarSessionTypes(doc);
        }

        [Obsolete("Используйте GetFuturesSessionWithTypes() — один запрос вместо двух")]
        public async Task<List<CalendarFuturesSessionDTO>> GetFuturesSession(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/session.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarFuturesSession(doc);
        }

        [Obsolete("Используйте GetFuturesSessionWithTypes() — один запрос вместо двух")]
        public async Task<List<CalendarSessionTypeDTO>> GetFuturesSessionTypes(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/session.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarSessionTypes(doc);
        }
        */

        // ── Фьючерсы ──────────────────────────────────

        public async Task<(List<CalendarFortsContractDTO> Forts, List<CalendarOptionsSeriesDTO> Options)>
            GetFuturesSecuritiesAll(CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/securities.json", cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            
            return ParsingCalendarUtf8.ParseFuturesSecurities(rentedArr.Span);
        }

        /*
        [Obsolete("Используйте GetFuturesSecuritiesAll() — один запрос вместо двух")]
        public async Task<List<CalendarFortsContractDTO>> GetFortsContracts(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/securities.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarFortsContracts(doc);
        }

        [Obsolete("Используйте GetFuturesSecuritiesAll() — один запрос вместо двух")]
        public async Task<List<CalendarOptionsSeriesDTO>> GetOptionsSeries(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/securities.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarOptionsSeries(doc);
        }
        */

        // ── Приостановленные (с cursor-пагинацией) ─────────────────────

        public async Task<List<CalendarSuspendedReasonDTO>> GetSuspendedReasons(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/securities/suspended/details.json", cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            var (_, reasons, _) = ParsingCalendarUtf8.ParseSuspendedWithReasons(rentedArr.Span);
            return reasons;
        }

        public async IAsyncEnumerable<List<CalendarSuspendedDTO>> GetSuspended(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync("/calendars/stock/securities/suspended/details.json", queryParams, cancellationToken);
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                var (page, _, cursor) = ParsingCalendarUtf8.ParseSuspendedWithReasons(rentedArr.Span);
                yield return page;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();
            }
        }

        // ── Изменения по ценным бумагам (с cursor-пагинацией) ──────────────

        public async Task<List<CalendarSecurityAttributeDTO>> GetSecurityAttributes(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/securities/changes.json", cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            var (_, attributes, _) = ParsingCalendarUtf8.ParseSecurityChangesWithAttributes(rentedArr.Span);
            return attributes;
        }

        public async IAsyncEnumerable<List<CalendarSecurityChangeDTO>> GetSecurityChanges(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            int pagesElapsed = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync("/calendars/stock/securities/changes.json", queryParams, cancellationToken);
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                var (page, _, cursor) = ParsingCalendarUtf8.ParseSecurityChangesWithAttributes(rentedArr.Span);
                yield return page;
                pagesElapsed++;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();
            }
        }

        // ── Инфраструктура ──────────────────────────────────────

        private async Task<HttpResponseMessage> SendRequestAsync(
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
