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

        public async Task<List<StockSecurityDTO>> GetInfoTradedStockAssets(string method)
        {
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return ParsingISS.ParseIssSecurityStock(JsonDocument.Parse(await response.Content.ReadAsStringAsync()));
             
        }

        

    }
}
