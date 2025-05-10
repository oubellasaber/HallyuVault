using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using System.Text.Json;

namespace HallyuVault.Etl.ApiKeyRotator.ScraperApi
{
    public interface IScraperApiKeyFactory : IApiKeyFactory<ScraperApiKey>
    {
    }

    public class ScraperApiKeyFactory : IScraperApiKeyFactory
    {
        private readonly HttpClient _httpClient;

        public ScraperApiKeyFactory(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async ValueTask<ScraperApiKey> CreateAsync(string apiKey)
        {
            if (apiKey is null)
                throw new ArgumentNullException(nameof(apiKey));

            if (apiKey.Length != 32 || !apiKey.All(c => char.IsAsciiHexDigit(c)))
            {
                throw new ArgumentException("The submitted api key does not match the expected format", nameof(apiKey));
            }

            var apiKeyInfo = await GetApiKeyInfoAsync(apiKey);

            return apiKeyInfo;
        }

        private async Task<ScraperApiKey> GetApiKeyInfoAsync(string apiKey)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.scraperapi.com/account?api_key={apiKey}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var total = root.GetProperty("requestLimit").GetInt32();
                var used = root.GetProperty("requestCount").GetInt32();
                var limit = root.GetProperty("concurrencyLimit").GetInt32();
                var active = root.GetProperty("concurrentRequests").GetInt32();
                var subscriptionDate = root.GetProperty("subscriptionDate").GetDateTime();

                var key = new ScraperApiKey(apiKey, total, used, limit, active, subscriptionDate);
                return key;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve API key information", ex);
            }
        }
    }
}