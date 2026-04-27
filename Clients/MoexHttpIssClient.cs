using History_DataMoex.Options;
using Microsoft.Extensions.Options;

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

        public async Task<string> GetMarketStockRaws(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GetRaws(string method, List<string>? queryParams = null)
        {
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;


            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadAsStringAsync();

        }
    }
}
