using History_DataMoex.Clients.Errors;
using History_DataMoex.Contracts.Dto;
using History_DataMoex.Contracts.Dto.Calendar;
using History_DataMoex.Contracts.Pagination;
using History_DataMoex.Infrastructure.Buffers;
using History_DataMoex.Mappers;
using History_DataMoex.Models;
using History_DataMoex.Options;
using History_DataMoex.Parsing;
using History_DataMoex.Parsing.Errors;
using History_DataMoex.RawCapture;
using History_DataMoex.RawStore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;

namespace History_DataMoex.Clients
{
    public class MoexHttpCalendarClient
    {
        private readonly MoexOptions _options;
        private readonly HttpClient _httpClient;
        private readonly IRawObjectStore _rawObjectStore;
        private readonly ILogger<MoexHttpCalendarClient> _logger;

        public MoexHttpCalendarClient(
            IOptions<MoexOptions> options,
            HttpClient httpClient,
            IRawObjectStore rawObjectStore,
            ILogger<MoexHttpCalendarClient> logger)
        {
            _options = options.Value;
            _httpClient = httpClient;
            _rawObjectStore = rawObjectStore;
            _logger = logger;
        }

        // ── Выходные дни ────────────────────────────────────────────

        public async Task<List<CalendarOffDaysAllDTO>> GetOffDaysAll(
            CancellationToken cancellationToken = default)
        {
            const string endpoint = "/calendars.json";
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(endpoint, cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                List<CalendarOffDaysAllDTO> result = ParsingCalendarUtf8.ParseOffDaysAll(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, endpoint, result.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return result;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }
        }

        public async Task<List<CalendarOffDaysMarketDTO>> GetStockOffDays(
            CancellationToken cancellationToken = default)
        {
            const string endpoint = "/calendars/stock.json";
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(endpoint, cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                List<CalendarOffDaysMarketDTO> result = ParsingCalendarUtf8.ParseOffDaysMarket(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, endpoint, result.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return result;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }
        }

        public async Task<List<CalendarOffDaysMarketDTO>> GetFuturesOffDays(
            CancellationToken cancellationToken = default)
        {
            const string endpoint = "/calendars/futures.json";
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(endpoint, cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                List<CalendarOffDaysMarketDTO> result = ParsingCalendarUtf8.ParseOffDaysMarket(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, endpoint, result.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return result;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }
        }

        // ── Сессии ────────────────────────────────────────────

        public async Task<(List<CalendarStockSessionDTO> Sessions, List<CalendarSessionTypeDTO> Types)>
            GetStockSessionWithTypes(CancellationToken cancellationToken = default)
        {
            const string endpoint = "/calendars/stock/session.json";
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(endpoint, cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                var result = ParsingCalendarUtf8.ParseStockSession(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, endpoint, result.Sessions.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return result;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }
        }

        public async Task<(List<CalendarFuturesSessionDTO> Sessions, List<CalendarSessionTypeDTO> Types)>
            GetFuturesSessionWithTypes(CancellationToken cancellationToken = default)
        {
            const string endpoint = "/calendars/futures/session.json";
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(endpoint, cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                var result = ParsingCalendarUtf8.ParseFuturesSession(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, endpoint, result.Sessions.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return result;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }
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
            const string endpoint = "/calendars/futures/securities.json";
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(endpoint, cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                var result = ParsingCalendarUtf8.ParseFuturesSecurities(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, endpoint, result.Forts.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return result;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }
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
            const string endpoint = "/calendars/stock/securities/suspended/details.json";
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(endpoint, cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                var (_, reasons, _) = ParsingCalendarUtf8.ParseSuspendedWithReasons(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, endpoint, reasons.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return reasons;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }
        }

        public async IAsyncEnumerable<List<CalendarSuspendedDTO>> GetSuspended(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            const string endpoint = "/calendars/stock/securities/suspended/details.json";
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            int pagesElapsed = 0;
            int totalRows = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                long pageStart = Stopwatch.GetTimestamp();
                using var response = await SendRequestAsync(endpoint, queryParams, cancellationToken);
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<CalendarSuspendedDTO> page;
                PaginationCursorDTO cursor;
                try
                {
                    var parsed = ParsingCalendarUtf8.ParseSuspendedWithReasons(rentedArr.Span);
                    page = parsed.Item1;
                    cursor = parsed.Item3;
                }
                catch (MoexSchemaMismatchException ex)
                {
                    MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                    throw;
                }
                pagesElapsed++;
                totalRows += page.Count;
                MoexLogMessages.PageReceived(_logger, endpoint, pagesElapsed, page.Count, Stopwatch.GetElapsedTime(pageStart));
                yield return page;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    MoexLogMessages.PaginationStopped(_logger, endpoint, step.StopReason!, pagesElapsed, totalRows);
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();
            }
        }

        // ── Изменения по ценным бумагам (с cursor-пагинацией) ──────────────

        public async Task<List<CalendarSecurityAttributeDTO>> GetSecurityAttributes(
            CancellationToken cancellationToken = default)
        {
            const string endpoint = "/calendars/stock/securities/changes.json";
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(endpoint, cancellationToken: cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                var (_, attributes, _) = ParsingCalendarUtf8.ParseSecurityChangesWithAttributes(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, endpoint, attributes.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return attributes;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }
        }

        public async IAsyncEnumerable<List<CalendarSecurityChangeDTO>> GetSecurityChanges(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            const string endpoint = "/calendars/stock/securities/changes.json";
            Dictionary<string, string> queryParams = new Dictionary<string, string>();

            int pagesElapsed = 0;
            int totalRows = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                long pageStart = Stopwatch.GetTimestamp();
                using var response = await SendRequestAsync(endpoint, queryParams, cancellationToken);
                int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
                using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    contentLength,
                    cancellationToken);

                List<CalendarSecurityChangeDTO> page;
                PaginationCursorDTO cursor;
                try
                {
                    var parsed = ParsingCalendarUtf8.ParseSecurityChangesWithAttributes(rentedArr.Span);
                    page = parsed.Item1;
                    cursor = parsed.Item3;
                }
                catch (MoexSchemaMismatchException ex)
                {
                    MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                    throw;
                }
                pagesElapsed++;
                totalRows += page.Count;
                MoexLogMessages.PageReceived(_logger, endpoint, pagesElapsed, page.Count, Stopwatch.GetElapsedTime(pageStart));
                yield return page;
                PaginationStep step = MoexCursorPagination.Next(cursor, pagesElapsed, _options.MaxPagesPerLoad);
                if (step.IsStop)
                {
                    MoexLogMessages.PaginationStopped(_logger, endpoint, step.StopReason!, pagesElapsed, totalRows);
                    break;
                }
                queryParams["start"] = step.NextStart.ToString();
            }
        }

        // ══════════════════════════════════════════════════════════════
        // Phase 8-C: internal raw методы — save-before-parse (Lock §2).
        // 8 методов = 3 page + 5 multi-table (2 с cursor).
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Internal raw метод: общие выходные дни (CalendarOffDayAll, Group C).
        /// Save-before-parse: raw payload сохраняется ДО парсинга (Lock §2).
        /// </summary>
        internal async Task<SourcePage<CalendarOffDayAll>> GetOffDaysAllRawAsync(
            CancellationToken ct = default)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_CALENDAR";
            const string endpoint = "/calendars.json";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            using var response = await SendRequestAsync(endpoint, cancellationToken: ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: endpoint,
                SourceTimezone: "Europe/Moscow",
                LoadJobId: loadJobId,
                RawObjectId: rawObjectId,
                FetchedAtUtc: fetchedAtUtc);

            RawObjectMeta rawMeta = await _rawObjectStore.SaveAsync(
                content: rentedArr.Memory,
                context: mapCtx,
                secId: string.Empty,
                fromDate: string.Empty,
                tillDate: string.Empty,
                ct: ct);

            if (rawMeta.RawObjectId != rawObjectId)
                throw new InvalidOperationException(
                    $"IRawObjectStore.SaveAsync returned different RawObjectId. Expected {rawObjectId}, got {rawMeta.RawObjectId}.");

            List<CalendarOffDaysAllDTO> dtos;
            try
            {
                dtos = ParsingCalendarUtf8.ParseOffDaysAll(rentedArr.Span);
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }

            List<CalendarOffDayAll> models = CalendarOffDayAllMapper.MapBatch(dtos, mapCtx, _logger, ct);

            MoexLogMessages.SinglePageReceived(_logger, endpoint, models.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourcePage<CalendarOffDayAll>
            {
                Items = models,
                NextCursor = null,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        /// <summary>
        /// Internal raw метод: выходные дни stock-рынка (CalendarMarketOffDay, Group C + market="stock").
        /// </summary>
        internal async Task<SourcePage<CalendarMarketOffDay>> GetStockOffDaysRawAsync(
            CancellationToken ct = default)
            => await GetMarketOffDaysRawCoreAsync("/calendars/stock.json", "stock", ct);

        /// <summary>
        /// Internal raw метод: выходные дни futures-рынка (CalendarMarketOffDay, Group C + market="futures").
        /// </summary>
        internal async Task<SourcePage<CalendarMarketOffDay>> GetFuturesOffDaysRawAsync(
            CancellationToken ct = default)
            => await GetMarketOffDaysRawCoreAsync("/calendars/futures.json", "futures", ct);

        private async Task<SourcePage<CalendarMarketOffDay>> GetMarketOffDaysRawCoreAsync(
            string endpoint,
            string market,
            CancellationToken ct)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_CALENDAR";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            using var response = await SendRequestAsync(endpoint, cancellationToken: ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: endpoint,
                SourceTimezone: "Europe/Moscow",
                LoadJobId: loadJobId,
                RawObjectId: rawObjectId,
                FetchedAtUtc: fetchedAtUtc);

            RawObjectMeta rawMeta = await _rawObjectStore.SaveAsync(
                content: rentedArr.Memory,
                context: mapCtx,
                secId: string.Empty,
                fromDate: string.Empty,
                tillDate: string.Empty,
                ct: ct);

            if (rawMeta.RawObjectId != rawObjectId)
                throw new InvalidOperationException(
                    $"IRawObjectStore.SaveAsync returned different RawObjectId. Expected {rawObjectId}, got {rawMeta.RawObjectId}.");

            List<CalendarOffDaysMarketDTO> dtos;
            try
            {
                dtos = ParsingCalendarUtf8.ParseOffDaysMarket(rentedArr.Span);
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }

            List<CalendarMarketOffDay> models = CalendarMarketOffDayMapper.MapBatch(dtos, market, mapCtx, _logger, ct);

            MoexLogMessages.SinglePageReceived(_logger, endpoint, models.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourcePage<CalendarMarketOffDay>
            {
                Items = models,
                NextCursor = null,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        /// <summary>
        /// Internal raw метод: расписание сессии stock-рынка + типы сессий.
        /// Multi-table cohesion (Lock §6): обе таблицы получают один RawObjectId.
        /// </summary>
        internal async Task<SourceMultiTable<CalendarStockSession, CalendarSessionType>>
            GetStockSessionWithTypesRawAsync(CancellationToken ct = default)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_CALENDAR";
            const string endpoint = "/calendars/stock/session.json";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            using var response = await SendRequestAsync(endpoint, cancellationToken: ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: endpoint,
                SourceTimezone: "Europe/Moscow",
                LoadJobId: loadJobId,
                RawObjectId: rawObjectId,
                FetchedAtUtc: fetchedAtUtc);

            RawObjectMeta rawMeta = await _rawObjectStore.SaveAsync(
                content: rentedArr.Memory,
                context: mapCtx,
                secId: string.Empty,
                fromDate: string.Empty,
                tillDate: string.Empty,
                ct: ct);

            if (rawMeta.RawObjectId != rawObjectId)
                throw new InvalidOperationException(
                    $"IRawObjectStore.SaveAsync returned different RawObjectId. Expected {rawObjectId}, got {rawMeta.RawObjectId}.");

            List<CalendarStockSessionDTO> sessionsDtos;
            List<CalendarSessionTypeDTO> typesDtos;
            try
            {
                var parsed = ParsingCalendarUtf8.ParseStockSession(rentedArr.Span);
                sessionsDtos = parsed.Sessions;
                typesDtos = parsed.Types;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }

            List<CalendarStockSession> mainItems = CalendarStockSessionMapper.MapBatch(sessionsDtos, mapCtx, _logger, ct);
            List<CalendarSessionType> secondaryItems = CalendarSessionTypeMapper.MapBatch(typesDtos, "stock", mapCtx, _logger, ct);

            MoexLogMessages.SinglePageReceived(_logger, endpoint, mainItems.Count + secondaryItems.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourceMultiTable<CalendarStockSession, CalendarSessionType>
            {
                MainItems = mainItems,
                SecondaryItems = secondaryItems,
                NextCursor = null,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        /// <summary>
        /// Internal raw метод: расписание сессии futures-рынка + типы сессий.
        /// Multi-table cohesion (Lock §6).
        /// </summary>
        internal async Task<SourceMultiTable<CalendarFuturesSession, CalendarSessionType>>
            GetFuturesSessionWithTypesRawAsync(CancellationToken ct = default)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_CALENDAR";
            const string endpoint = "/calendars/futures/session.json";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            using var response = await SendRequestAsync(endpoint, cancellationToken: ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: endpoint,
                SourceTimezone: "Europe/Moscow",
                LoadJobId: loadJobId,
                RawObjectId: rawObjectId,
                FetchedAtUtc: fetchedAtUtc);

            RawObjectMeta rawMeta = await _rawObjectStore.SaveAsync(
                content: rentedArr.Memory,
                context: mapCtx,
                secId: string.Empty,
                fromDate: string.Empty,
                tillDate: string.Empty,
                ct: ct);

            if (rawMeta.RawObjectId != rawObjectId)
                throw new InvalidOperationException(
                    $"IRawObjectStore.SaveAsync returned different RawObjectId. Expected {rawObjectId}, got {rawMeta.RawObjectId}.");

            List<CalendarFuturesSessionDTO> sessionsDtos;
            List<CalendarSessionTypeDTO> typesDtos;
            try
            {
                var parsed = ParsingCalendarUtf8.ParseFuturesSession(rentedArr.Span);
                sessionsDtos = parsed.Sessions;
                typesDtos = parsed.Types;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }

            List<CalendarFuturesSession> mainItems = CalendarFuturesSessionMapper.MapBatch(sessionsDtos, mapCtx, _logger, ct);
            List<CalendarSessionType> secondaryItems = CalendarSessionTypeMapper.MapBatch(typesDtos, "futures", mapCtx, _logger, ct);

            MoexLogMessages.SinglePageReceived(_logger, endpoint, mainItems.Count + secondaryItems.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourceMultiTable<CalendarFuturesSession, CalendarSessionType>
            {
                MainItems = mainItems,
                SecondaryItems = secondaryItems,
                NextCursor = null,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        /// <summary>
        /// Internal raw метод: справочник Forts contracts + Options series.
        /// Multi-table, без cursor (Lock §6).
        /// </summary>
        internal async Task<SourceMultiTable<CalendarFortsContract, CalendarOptionsSeries>>
            GetFuturesSecuritiesAllRawAsync(CancellationToken ct = default)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_CALENDAR";
            const string endpoint = "/calendars/futures/securities.json";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            using var response = await SendRequestAsync(endpoint, cancellationToken: ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: endpoint,
                SourceTimezone: "Europe/Moscow",
                LoadJobId: loadJobId,
                RawObjectId: rawObjectId,
                FetchedAtUtc: fetchedAtUtc);

            RawObjectMeta rawMeta = await _rawObjectStore.SaveAsync(
                content: rentedArr.Memory,
                context: mapCtx,
                secId: string.Empty,
                fromDate: string.Empty,
                tillDate: string.Empty,
                ct: ct);

            if (rawMeta.RawObjectId != rawObjectId)
                throw new InvalidOperationException(
                    $"IRawObjectStore.SaveAsync returned different RawObjectId. Expected {rawObjectId}, got {rawMeta.RawObjectId}.");

            List<CalendarFortsContractDTO> fortsDtos;
            List<CalendarOptionsSeriesDTO> optionsDtos;
            try
            {
                var parsed = ParsingCalendarUtf8.ParseFuturesSecurities(rentedArr.Span);
                fortsDtos = parsed.Forts;
                optionsDtos = parsed.Options;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }

            List<CalendarFortsContract> mainItems = CalendarFortsContractMapper.MapBatch(fortsDtos, mapCtx, _logger, ct);
            List<CalendarOptionsSeries> secondaryItems = CalendarOptionsSeriesMapper.MapBatch(optionsDtos, mapCtx, _logger, ct);

            MoexLogMessages.SinglePageReceived(_logger, endpoint, mainItems.Count + secondaryItems.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourceMultiTable<CalendarFortsContract, CalendarOptionsSeries>
            {
                MainItems = mainItems,
                SecondaryItems = secondaryItems,
                NextCursor = null,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        /// <summary>
        /// Internal raw метод: приостановки торгов + справочник причин (multi-table с cursor).
        /// Caller использует NextCursor + MoexCursorPagination.Next для chaining (передаёт NextStart).
        /// Multi-table cohesion (Lock §6).
        /// </summary>
        internal async Task<SourceMultiTable<CalendarSuspension, CalendarSuspensionReason>>
            GetSuspendedWithReasonsRawAsync(int start = 0, CancellationToken ct = default)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_CALENDAR";
            const string endpoint = "/calendars/stock/securities/suspended/details.json";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            Dictionary<string, string> queryParams = new();
            if (start > 0)
                queryParams["start"] = start.ToString(CultureInfo.InvariantCulture);

            using var response = await SendRequestAsync(endpoint, queryParams, ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: endpoint,
                SourceTimezone: "Europe/Moscow",
                LoadJobId: loadJobId,
                RawObjectId: rawObjectId,
                FetchedAtUtc: fetchedAtUtc);

            RawObjectMeta rawMeta = await _rawObjectStore.SaveAsync(
                content: rentedArr.Memory,
                context: mapCtx,
                secId: string.Empty,
                fromDate: string.Empty,
                tillDate: string.Empty,
                ct: ct);

            if (rawMeta.RawObjectId != rawObjectId)
                throw new InvalidOperationException(
                    $"IRawObjectStore.SaveAsync returned different RawObjectId. Expected {rawObjectId}, got {rawMeta.RawObjectId}.");

            List<CalendarSuspendedDTO> suspendedDtos;
            List<CalendarSuspendedReasonDTO> reasonsDtos;
            PaginationCursorDTO cursor;
            try
            {
                var parsed = ParsingCalendarUtf8.ParseSuspendedWithReasons(rentedArr.Span);
                suspendedDtos = parsed.Suspended;
                reasonsDtos = parsed.Reasons;
                cursor = parsed.Cursor;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }

            List<CalendarSuspension> mainItems = CalendarSuspensionMapper.MapBatch(suspendedDtos, mapCtx, _logger, ct);
            List<CalendarSuspensionReason> secondaryItems = CalendarSuspensionReasonMapper.MapBatch(reasonsDtos, mapCtx, _logger, ct);

            PaginationCursorDTO? nextCursor =
                (cursor.Index.HasValue || cursor.Total.HasValue || cursor.PageSize.HasValue) ? cursor : null;

            MoexLogMessages.SinglePageReceived(_logger, endpoint, mainItems.Count + secondaryItems.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourceMultiTable<CalendarSuspension, CalendarSuspensionReason>
            {
                MainItems = mainItems,
                SecondaryItems = secondaryItems,
                NextCursor = nextCursor,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        /// <summary>
        /// Internal raw метод: изменения атрибутов бумаг + справочник атрибутов (multi-table с cursor).
        /// Caller chaining через NextCursor + MoexCursorPagination.Next.NextStart.
        /// Multi-table cohesion (Lock §6).
        /// </summary>
        internal async Task<SourceMultiTable<CalendarSecurityChange, CalendarSecurityAttribute>>
            GetSecurityChangesWithAttributesRawAsync(int start = 0, CancellationToken ct = default)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_CALENDAR";
            const string endpoint = "/calendars/stock/securities/changes.json";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            Dictionary<string, string> queryParams = new();
            if (start > 0)
                queryParams["start"] = start.ToString(CultureInfo.InvariantCulture);

            using var response = await SendRequestAsync(endpoint, queryParams, ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: endpoint,
                SourceTimezone: "Europe/Moscow",
                LoadJobId: loadJobId,
                RawObjectId: rawObjectId,
                FetchedAtUtc: fetchedAtUtc);

            RawObjectMeta rawMeta = await _rawObjectStore.SaveAsync(
                content: rentedArr.Memory,
                context: mapCtx,
                secId: string.Empty,
                fromDate: string.Empty,
                tillDate: string.Empty,
                ct: ct);

            if (rawMeta.RawObjectId != rawObjectId)
                throw new InvalidOperationException(
                    $"IRawObjectStore.SaveAsync returned different RawObjectId. Expected {rawObjectId}, got {rawMeta.RawObjectId}.");

            List<CalendarSecurityChangeDTO> changesDtos;
            List<CalendarSecurityAttributeDTO> attributesDtos;
            PaginationCursorDTO cursor;
            try
            {
                var parsed = ParsingCalendarUtf8.ParseSecurityChangesWithAttributes(rentedArr.Span);
                changesDtos = parsed.Changes;
                attributesDtos = parsed.Attributes;
                cursor = parsed.Cursor;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, endpoint, "schema_mismatch", ex.Message);
                throw;
            }

            List<CalendarSecurityChange> mainItems = CalendarSecurityChangeMapper.MapBatch(changesDtos, mapCtx, _logger, ct);
            List<CalendarSecurityAttribute> secondaryItems = CalendarSecurityAttributeMapper.MapBatch(attributesDtos, mapCtx, _logger, ct);

            PaginationCursorDTO? nextCursor =
                (cursor.Index.HasValue || cursor.Total.HasValue || cursor.PageSize.HasValue) ? cursor : null;

            MoexLogMessages.SinglePageReceived(_logger, endpoint, mainItems.Count + secondaryItems.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourceMultiTable<CalendarSecurityChange, CalendarSecurityAttribute>
            {
                MainItems = mainItems,
                SecondaryItems = secondaryItems,
                NextCursor = nextCursor,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        // ── Инфраструктура ──────────────────────────────────────

        private async Task<HttpResponseMessage> SendRequestAsync(
            string method,
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            string requestUrl = _options.ApimBaseUrl + method;
            queryParams ??= new Dictionary<string, string>();
            if (queryParams.Count > 0)
            {
                QueryString queryString = QueryString.Create(queryParams);
                requestUrl += queryString.ToString();
            }
            EnsureApiKeyConfigured();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.AlgKey}");
            try
            {
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                HttpClientHelpers.EnsureSuccessOrThrow(response, method);
                return response;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                var timeoutEx = new MoexTimeoutException($"MOEX request timeout for {method}", method, "http_client", _options.RequestTimeout, ex);
                MoexLogMessages.RequestFailed(_logger, timeoutEx, MoexLogSources.Calendar, method, timeoutEx.ErrorCategory, null, timeoutEx.TimeoutSource, timeoutEx.Message);
                throw timeoutEx;
            }
            catch (TimeoutRejectedException ex)
            {
                var timeoutEx = new MoexTimeoutException($"MOEX attempt timeout for {method}", method, "polly_attempt", null, ex);
                MoexLogMessages.RequestFailed(_logger, timeoutEx, MoexLogSources.Calendar, method, timeoutEx.ErrorCategory, null, timeoutEx.TimeoutSource, timeoutEx.Message);
                throw timeoutEx;
            }
            catch (MoexHttpException ex)
            {
                MoexLogMessages.RequestFailed(_logger, ex, MoexLogSources.Calendar, method, ex.ErrorCategory, (HttpStatusCode?)ex.StatusCode, null, ex.Message);
                throw;
            }
        }

        private void EnsureApiKeyConfigured()
        {
            if (string.IsNullOrWhiteSpace(_options.AlgKey))
            {
                throw new InvalidOperationException(
                    "MOEX ALGOPACK API key is not configured. Set MoexAlg:Key via user-secrets or environment variable.");
            }
        }
    }
}
