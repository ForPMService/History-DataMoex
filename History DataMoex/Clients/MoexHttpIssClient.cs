using History_DataMoex.Clients.Errors;
using History_DataMoex.Contracts.Dto.Iss;
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
using System.Net;


namespace History_DataMoex.Clients
{
    public class MoexHttpIssClient
    {
        private readonly MoexIssOptions _options;
        private readonly HttpClient _httpClient;
        private readonly IRawObjectStore _rawObjectStore;
        private readonly ILogger<MoexHttpIssClient> _logger;

        public MoexHttpIssClient(
            IOptions<MoexIssOptions> options,
            HttpClient httpClient,
            IRawObjectStore rawObjectStore,
            ILogger<MoexHttpIssClient> logger)
        {
            _options = options.Value;
            _httpClient = httpClient;
            _rawObjectStore = rawObjectStore;
            _logger = logger;
        }
        /// <summary>
        /// Diagnostic only. Does not use typed error handling (no MoexHttpException hierarchy,
        /// no EnsureSuccessOrThrow, no structured logging).
        /// Do NOT use for production raw capture. Phase 8 uses SendRequestAsync-based path
        /// in GetInfoTradedStockAssetsRaw / GetInfoTradedFuturesAssetsRaw (added in Phase 8-C).
        /// Used by DebugEndpoints only. Reliability fix: separate cleanup task after Phase 8-D.
        /// Lock §12.
        /// </summary>
        public async Task<string> GetRaw(
            string method,
            CancellationToken cancellationToken = default)
        {
            string requestUrl = _options.BaseUrl + method;
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        public async Task<List<StockSecurityDTO>> GetInfoTradedStockAssets(
            string method,
            CancellationToken cancellationToken = default)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(method, cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                List<StockSecurityDTO> result = ParsingIssUtf8.ParseIssSecurityStock(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, method, result.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return result;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, method, "schema_mismatch", ex.Message);
                throw;
            }
        }

        public async Task<List<FuturesSecurityDTO>> GetInfoTradedFuturesAssets(
            string method,
            CancellationToken cancellationToken = default)
        {
            long startTimestamp = Stopwatch.GetTimestamp();
            using var response = await SendRequestAsync(method, cancellationToken);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken),
                contentLength,
                cancellationToken);
            try
            {
                List<FuturesSecurityDTO> result = ParsingIssUtf8.ParseIssSecurityFutures(rentedArr.Span);
                MoexLogMessages.SinglePageReceived(_logger, method, result.Count, Stopwatch.GetElapsedTime(startTimestamp));
                return result;
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, method, "schema_mismatch", ex.Message);
                throw;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // Phase 8-C: internal raw методы — save-before-parse (Lock §2).
        // 2 метода (ISS — page, без cursor; snapshot policy Lock §5/§6).
        // Имена БЕЗ Async — convention из XML doc GetRaw (Lock §12) и существующих public методов.
        // ══════════════════════════════════════════════════════════════

        /// <summary>
        /// Internal raw метод: snapshot справочника stock securities для конкретного board (ISS lite).
        /// Caller подаёт полный relative URL включая board (например,
        /// "/engines/stock/markets/shares/boards/tqbr/securities.json").
        /// Save-before-parse + snapshot policy (Lock §2, §5, §6).
        /// </summary>
        internal async Task<SourcePage<IssStockSecurity>> GetInfoTradedStockAssetsRaw(
            string method,
            CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Method (URL) cannot be null or empty.", nameof(method));

            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_ISS";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            using var response = await SendRequestAsync(method, ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: method,
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

            List<StockSecurityDTO> dtos;
            try
            {
                dtos = ParsingIssUtf8.ParseIssSecurityStock(rentedArr.Span);
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, method, "schema_mismatch", ex.Message);
                throw;
            }

            List<IssStockSecurity> models = IssStockSecurityMapper.MapBatch(dtos, mapCtx, _logger, ct);

            MoexLogMessages.SinglePageReceived(_logger, method, models.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourcePage<IssStockSecurity>
            {
                Items = models,
                NextCursor = null,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        /// <summary>
        /// Internal raw метод: snapshot справочника futures securities для конкретного board (ISS lite).
        /// Save-before-parse + snapshot policy (Lock §2, §5, §6).
        /// </summary>
        internal async Task<SourcePage<IssFuturesSecurity>> GetInfoTradedFuturesAssetsRaw(
            string method,
            CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("Method (URL) cannot be null or empty.", nameof(method));

            long startTimestamp = Stopwatch.GetTimestamp();
            const string sourceCode = "MOEX_ISS";
            Guid loadJobId = Guid.CreateVersion7();
            DateTime fetchedAtUtc = DateTime.UtcNow;

            using var response = await SendRequestAsync(method, ct);
            int contentLength = (int)(response.Content.Headers.ContentLength ?? 1_048_576);
            using var rentedArr = await RentedBuffer.RentFromStreamAsync(
                await response.Content.ReadAsStreamAsync(ct), contentLength, ct);

            Guid rawObjectId = Guid.CreateVersion7();
            MapContext mapCtx = new(
                SourceCode: sourceCode,
                Endpoint: method,
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

            List<FuturesSecurityDTO> dtos;
            try
            {
                dtos = ParsingIssUtf8.ParseIssSecurityFutures(rentedArr.Span);
            }
            catch (MoexSchemaMismatchException ex)
            {
                MoexLogMessages.ParseFailed(_logger, ex, method, "schema_mismatch", ex.Message);
                throw;
            }

            List<IssFuturesSecurity> models = IssFuturesSecurityMapper.MapBatch(dtos, mapCtx, _logger, ct);

            MoexLogMessages.SinglePageReceived(_logger, method, models.Count, Stopwatch.GetElapsedTime(startTimestamp));

            return new SourcePage<IssFuturesSecurity>
            {
                Items = models,
                NextCursor = null,
                RawObjectId = rawObjectId,
                FetchedAtUtc = fetchedAtUtc,
            };
        }

        private async Task<HttpResponseMessage> SendRequestAsync(string method, CancellationToken cancellationToken)
        {
            string requestUrl = _options.BaseUrl + method;
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            try
            {
                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                HttpClientHelpers.EnsureSuccessOrThrow(response, method);
                return response;
            }
            catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                var timeoutEx = new MoexTimeoutException($"MOEX request timeout for {method}", method, "http_client", _options.RequestTimeout, ex);
                MoexLogMessages.RequestFailed(_logger, timeoutEx, MoexLogSources.Iss, method, timeoutEx.ErrorCategory, null, timeoutEx.TimeoutSource, timeoutEx.Message);
                throw timeoutEx;
            }
            catch (TimeoutRejectedException ex)
            {
                var timeoutEx = new MoexTimeoutException($"MOEX attempt timeout for {method}", method, "polly_attempt", null, ex);
                MoexLogMessages.RequestFailed(_logger, timeoutEx, MoexLogSources.Iss, method, timeoutEx.ErrorCategory, null, timeoutEx.TimeoutSource, timeoutEx.Message);
                throw timeoutEx;
            }
            catch (MoexHttpException ex)
            {
                MoexLogMessages.RequestFailed(_logger, ex, MoexLogSources.Iss, method, ex.ErrorCategory, (HttpStatusCode?)ex.StatusCode, (ex as MoexTimeoutException)?.TimeoutSource, ex.Message);
                throw;
            }
        }

    }
}
