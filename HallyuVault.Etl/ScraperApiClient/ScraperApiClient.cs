using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HallyuVault.Etl.ScraperApiClient
{
    public class ScraperApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ScraperApiOptions _options;

        public ScraperApiClient(HttpClient httpClient, IOptions<ScraperApiOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<HttpResponseMessage> SendAsync(ScraperRequest request)
        {
            var httpRequest = new HttpRequestMessage(request.Method, request.Url)
            {
                Content = request.Content
            };

            foreach (var header in request.Headers)
                httpRequest.Headers.Add(header.Key, header.Value);

            return await _httpClient.SendAsync(httpRequest);
        }

        public async Task<string> GetAccountInfoAsync(string apiKey)
        {
            var url = $"{_options.BaseUrl}{_options.AccountEndpoint}?api_key={apiKey}";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<int> GetActualCostFromApi(string apiKey, Uri targetUrl)
        {
            var url = $"{_options.BaseUrl}{_options.UrlCostEndpoint}?url={Uri.EscapeDataString(targetUrl.ToString())}&api_key={apiKey}";
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.TryGetProperty("credits", out var credits) ? credits.GetInt32() : -1;
        }

        public int GetCostFromResponse(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("sa-credit-cost", out var values) &&
                int.TryParse(values.FirstOrDefault(), out var cost))
            {
                return cost;
            }

            return 0;
        }

        public int EstimateCost(RequestParameters p)
        {
            if (p.Premium && p.RenderJavaScript) return 25;
            int cost = 1;
            if (p.Premium) cost += 10;
            if (p.RenderJavaScript) cost += 10;
            return cost;
        }
    }
}