using History_DataMoex.DataTransfers;
using History_DataMoex.DataTransfers.Calendar;
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

        public async Task<List<CalendarOffDaysAllDTO>> GetOffDaysAll()
        {
            var response = await SendRequest("/calendars.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarOffDaysAll(doc);
        }

        public async Task<List<CalendarOffDaysMarketDTO>> GetStockOffDays()
        {
            var response = await SendRequest("/calendars/stock.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarOffDaysMarket(doc);
        }

        public async Task<List<CalendarOffDaysMarketDTO>> GetFuturesOffDays()
        {
            var response = await SendRequest("/calendars/futures.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarOffDaysMarket(doc);
        }

        // ── Сессии ────────────────────────────────────────────

        public async Task<List<CalendarStockSessionDTO>> GetStockSession()
        {
            var response = await SendRequest("/calendars/stock/session.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarStockSession(doc);
        }

        public async Task<List<CalendarSessionTypeDTO>> GetStockSessionTypes()
        {
            var response = await SendRequest("/calendars/stock/session.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarSessionTypes(doc);
        }

        public async Task<List<CalendarFuturesSessionDTO>> GetFuturesSession()
        {
            var response = await SendRequest("/calendars/futures/session.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarFuturesSession(doc);
        }

        public async Task<List<CalendarSessionTypeDTO>> GetFuturesSessionTypes()
        {
            var response = await SendRequest("/calendars/futures/session.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarSessionTypes(doc);
        }

        // ── Фьючерсы ──────────────────────────────────

        public async Task<List<CalendarFortsContractDTO>> GetFortsContracts()
        {
            var response = await SendRequest("/calendars/futures/securities.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarFortsContracts(doc);
        }

        public async Task<List<CalendarOptionsSeriesDTO>> GetOptionsSeries()
        {
            var response = await SendRequest("/calendars/futures/securities.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarOptionsSeries(doc);
        }

        // ── Приостановленные (с cursor-пагинацией) ─────────────────────

        public async Task<List<CalendarSuspendedReasonDTO>> GetSuspendedReasons()
        {
            var response = await SendRequest("/calendars/stock/securities/suspended/details.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarSuspendedReasons(doc);
        }

        public async Task<List<CalendarSuspendedDTO>> GetSuspended()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            List<CalendarSuspendedDTO> all = new List<CalendarSuspendedDTO>();

            while (true)
            {
                var response = await SendRequest("/calendars/stock/securities/suspended/details.json", queryParams);
                using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

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

        public async Task<List<CalendarSecurityAttributeDTO>> GetSecurityAttributes()
        {
            var response = await SendRequest("/calendars/stock/securities/changes.json");
            using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingCalendar.ParseCalendarSecurityAttributes(doc);
        }

        public async Task<List<CalendarSecurityChangeDTO>> GetSecurityChanges()
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            List<CalendarSecurityChangeDTO> all = new List<CalendarSecurityChangeDTO>();

            while (true)
            {
                var response = await SendRequest("/calendars/stock/securities/changes.json", queryParams);
                using JsonDocument doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

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

        private async Task<HttpResponseMessage> SendRequest(string method, Dictionary<string, string>? queryParams = null)
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
            response.EnsureSuccessStatusCode();
            return response;
        }
    }
}
