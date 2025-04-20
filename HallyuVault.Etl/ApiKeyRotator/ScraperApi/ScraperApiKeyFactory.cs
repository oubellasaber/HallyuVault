using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.ScraperApi;
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

        public async ValueTask<Result<ScraperApiKey>> CreateAsync(string apiKey)
        {
            if (apiKey.Length != 32 || apiKey.All(c => char.IsAsciiHexDigit(c)))
            {
                return Result.Failure<ScraperApiKey>(new Error("ScraperApiKey.InvalidFormat", "The submitted api key does not match the expected format"));
            }

            var apiKeyInfo = await GetApiKeyInfoAsync(apiKey);

            return apiKeyInfo;
        }

        private async Task<Result<ScraperApiKey>> GetApiKeyInfoAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length != 32)
            {
                return Result.Failure<ScraperApiKey>(new Error("ScraperApi.InvalidKey", "API key format is invalid."));
            }

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
                return Result.Success(key);
            }
            catch (Exception ex)
            {
                return Result.Failure<ScraperApiKey>(new Error("ScraperApi.Exception", ex.Message));
            }
        }
    }
}