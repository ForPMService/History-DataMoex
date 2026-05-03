using History_DataMoex.DataTransfers;
using History_DataMoex.Options;
using History_DataMoex.Parsing;
using Microsoft.Extensions.Options;
using System.Text.Json;

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

        public async Task<List<CandlesDTO>> GetCandles(string method, List<string>? queryParams = null)
        {
            string baseUrl = _options.BaseUrl;
            string requestUrl = baseUrl + method;


            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {_options.Key}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            return ParsingALG.ParseAlgCandles(JsonDocument.Parse(await response.Content.ReadAsStringAsync()));
        }
    }
}
