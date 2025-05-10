using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using HallyuVault.Etl.ScraperApiClient;

namespace HallyuVault.Etl.ApiKeyRotator.ScraperApi
{
    public class ScraperApiProxy
    {
        private readonly IApiKeyManager<ScraperApiKey, int> _apiKeyManager;
        private readonly ScraperApiClient.ScraperApiClient _scraperClient;
        private readonly RequestBuilder _builder;
        private readonly HttpClient _httpClient;

        public ScraperApiProxy(
            IApiKeyManager<ScraperApiKey, int> keyManager,
            ScraperApiClient.ScraperApiClient scraperClient,
            RequestBuilder builder,
            HttpClient httpClient)
        {
            _apiKeyManager = keyManager;
            _scraperClient = scraperClient;
            _builder = builder;
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> SendAsync(Uri url, StringContent? content = null, HttpMethod? method = null)
        {
            method ??= HttpMethod.Post;

            _builder.ForUrl(url).WithPremium();
            int estimated = _scraperClient.EstimateCost(_builder.Parameters);
            var apiKey = _apiKeyManager.Get(estimated);

            if (apiKey != null)
            {
                var scraperRequest = _builder.WithApiKey(apiKey.Key)
                                             .Build(method, content);

                var scraperResponse = await _scraperClient.SendAsync(scraperRequest);
                int actual = _scraperClient.GetCostFromResponse(scraperResponse);

                if (scraperResponse.IsSuccessStatusCode)
                    apiKey.UpdateConsumedCredits(estimated, actual);
                else
                    apiKey.UpdateConsumedCredits(-estimated);

                return scraperResponse;
            }

            // No usable API key: fallback to plain HttpClient
            var fallbackRequest = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            return await _httpClient.SendAsync(fallbackRequest);
        }
    }
}
