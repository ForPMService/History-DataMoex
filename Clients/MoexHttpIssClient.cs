using History_DataMoex.DataTransfers;
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
        public async Task<string> GetRaw(string method)
        {
            string requestUrl = _options.BaseUrl + method;
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<List<StockSecurityDTO>> GetInfoTradedStockAssets(string method)
        {
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            using var jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingISS.ParseIssSecurityStock(jsonDocument);
        }

        public async Task<List<FuturesSecurityDTO>> GetInfoTradedFuturesAssets(string method)
        {
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            using var jsonDocument = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return ParsingISS.ParseIssSecurityFutures(jsonDocument);

        }

    }
}
