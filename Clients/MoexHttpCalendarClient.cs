using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Contracts.Pagination;
using History_DataMoex.Options;
using History_DataMoex.Parsing;
using Microsoft.Extensions.Options;
using System.Text.Json;

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
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarOffDaysAll(doc);
        }

        public async Task<List<CalendarOffDaysMarketDTO>> GetStockOffDays(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarOffDaysMarket(doc);
        }

        public async Task<List<CalendarOffDaysMarketDTO>> GetFuturesOffDays(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarOffDaysMarket(doc);
        }

        // ── Сессии ────────────────────────────────────────────

        public async Task<List<CalendarStockSessionDTO>> GetStockSession(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/session.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarStockSession(doc);
        }

        public async Task<List<CalendarSessionTypeDTO>> GetStockSessionTypes(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/session.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarSessionTypes(doc);
        }

        public async Task<List<CalendarFuturesSessionDTO>> GetFuturesSession(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/session.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarFuturesSession(doc);
        }

        public async Task<List<CalendarSessionTypeDTO>> GetFuturesSessionTypes(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/session.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarSessionTypes(doc);
        }

        // ── Фьючерсы ──────────────────────────────────

        public async Task<List<CalendarFortsContractDTO>> GetFortsContracts(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/securities.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarFortsContracts(doc);
        }

        public async Task<List<CalendarOptionsSeriesDTO>> GetOptionsSeries(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/futures/securities.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarOptionsSeries(doc);
        }

        // ── Приостановленные (с cursor-пагинацией) ─────────────────────

        public async Task<List<CalendarSuspendedReasonDTO>> GetSuspendedReasons(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/securities/suspended/details.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarSuspendedReasons(doc);
        }

        public async Task<List<CalendarSuspendedDTO>> GetSuspended(
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            List<CalendarSuspendedDTO> all = new List<CalendarSuspendedDTO>();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync("/calendars/stock/securities/suspended/details.json", queryParams, cancellationToken);
                using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

                List<CalendarSuspendedDTO> page = ParsingCalendar.ParseCalendarSuspended(doc);
                PaginationCursorDTO cursor = ParsingCalendar.ParseCursor(doc, "suspended.cursor");
                all.AddRange(page);

                if (cursor.Index + cursor.PageSize >= cursor.Total)
                {
                    break;
                }

                queryParams["start"] = (cursor.Index!.Value + cursor.PageSize!.Value).ToString();
            }

            return all;
        }

        // ── Изменения по ценным бумагам (с cursor-пагинацией) ──────────────

        public async Task<List<CalendarSecurityAttributeDTO>> GetSecurityAttributes(
            CancellationToken cancellationToken = default)
        {
            using var response = await SendRequestAsync("/calendars/stock/securities/changes.json", cancellationToken: cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingCalendar.ParseCalendarSecurityAttributes(doc);
        }

        public async Task<List<CalendarSecurityChangeDTO>> GetSecurityChanges(
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            List<CalendarSecurityChangeDTO> all = new List<CalendarSecurityChangeDTO>();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var response = await SendRequestAsync("/calendars/stock/securities/changes.json", queryParams, cancellationToken);
                using JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

                List<CalendarSecurityChangeDTO> page = ParsingCalendar.ParseCalendarSecurityChanges(doc);
                PaginationCursorDTO cursor = ParsingCalendar.ParseCursor(doc, "securities.cursor");
                all.AddRange(page);

                if (cursor.Index + cursor.PageSize >= cursor.Total)
                {
                    break;
                }

                queryParams["start"] = (cursor.Index!.Value + cursor.PageSize!.Value).ToString();
            }

            return all;
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
            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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
