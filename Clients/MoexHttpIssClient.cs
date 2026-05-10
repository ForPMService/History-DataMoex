using History_DataMoex.Contracts.Dto.Iss;
using History_DataMoex.Options;
using History_DataMoex.Parsing;
using Microsoft.Extensions.Options;
using System.Text.Json;


namespace History_DataMoex.Clients
{
    public class MoexHttpIssClient
    {
        private readonly MoexIssOptions _options;
        private readonly HttpClient _httpClient;

        public MoexHttpIssClient(IOptions<MoexIssOptions> options, HttpClient httpClient)
        {
            _options = options.Value;
            _httpClient = httpClient;
        }
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
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingISS.ParseIssSecurityStock(jsonDocument);
        }

        public async Task<List<FuturesSecurityDTO>> GetInfoTradedFuturesAssets(
            string method,
            CancellationToken cancellationToken = default)
        {
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            using var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            return ParsingISS.ParseIssSecurityFutures(jsonDocument);

        }

        

    }
}
