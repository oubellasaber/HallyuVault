using System.Text.Json;

namespace HallyuVault.Etl.ScraperApiClient
{
    public class ScraperApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ScraperApiOptions _options;

        public ScraperApiClient(HttpClient httpClient, ScraperApiOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<HttpResponseMessage> GetAsync(RequestParameters parameters)
        {
            var requestUrl = BuildRequestUrl(parameters);
            var response = await _httpClient.GetAsync(requestUrl);

            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(RequestParameters parameters, StringContent content)
        {
            var requestUrl = BuildRequestUrl(parameters);
            var response = await _httpClient.PostAsync(requestUrl, content);
            return response;
        }

        private string BuildRequestUrl(RequestParameters parameters, string? endpoint = null)
        {
            if (endpoint == "account")
            {
                return $"{_options.BaseUrl}{_options.AccountEndpoint}?api_key={parameters.ApiKey}";
            }

            var queryParams = new Dictionary<string, string>
            {
                ["api_key"] = parameters.ApiKey
            };

            var url = parameters.Url.ToString();
            if (!string.IsNullOrEmpty(url))
                queryParams["url"] = url;

            if (parameters.RenderJavaScript)
                queryParams["render"] = "true";

            if (parameters.Premium)
                queryParams["premium"] = "true";

            var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            var path = string.IsNullOrEmpty(endpoint) ? "" : $"/{endpoint}";

            return $"{path}?{queryString}";
        }

        public async Task<string> GetAccountInfoAsync()
        {
            var requestUrl = BuildRequestUrl(null!, "account");
            var response = await _httpClient.GetAsync(requestUrl);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<int> GetActualCostFromApi(RequestParameters parameters)
        {
            var targetRequestUrl = BuildRequestUrl(parameters);
            var requestUrl = $"{_options.BaseUrl}{_options.UrlCostEndpoint}?url={targetRequestUrl}";

            var response = await _httpClient.GetAsync(requestUrl);
            var content = await response.Content.ReadAsStringAsync();

            using (JsonDocument document = JsonDocument.Parse(content))
            {
                if (document.RootElement.TryGetProperty("credits", out var creditsElement))
                {
                    return creditsElement.GetInt32();
                }
            }

            return -1;
        }

        public int GetCostFromResponse(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("sa-credit-cost", out var values))
            {
                if (int.TryParse(values.FirstOrDefault(), out var cost))
                {
                    return cost;
                }
            }

            return 0;
        }

        public int GetEstimatedCreditCost(RequestParameters parameters)
        {
            //if (parameters.ContainsKey("ultra_premium"))
            //    cost = parameters.ContainsKey("render") ? 75 : 30;
            int cost = 1;

            if (parameters.Premium && parameters.RenderJavaScript)
                cost = 25;
            else
            {
                if (parameters.Premium) cost += 10;
                if (parameters.RenderJavaScript) cost += 10;
            }

            return cost;
        }
    }
}
