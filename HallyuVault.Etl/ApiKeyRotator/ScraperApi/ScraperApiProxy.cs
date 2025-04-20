using HallyuVault.Etl.ScraperApiClient;

namespace HallyuVault.Etl.ApiKeyRotator.ScraperApi
{
    public class ScraperApiProxy
    {
        private readonly ScraperApiKeyManager _scraperApiKeyManager;
        private readonly ScraperApiClient.ScraperApiClient _scraperApiClient;

        public ScraperApiProxy(
            ScraperApiKeyManager scraperApiKeyManager,
            ScraperApiClient.ScraperApiClient scraperApiClient)
        {
            _scraperApiKeyManager = scraperApiKeyManager;
            _scraperApiClient = scraperApiClient;
        }

        public async Task<HttpResponseMessage> RequestAsync(RequestParameters parameters)
        {
            int estimatedCost = _scraperApiClient.GetEstimatedCreditCost(parameters);
            var apiKeyResult = _scraperApiKeyManager.Get(estimatedCost);

            if (apiKeyResult.IsFailure)
            {
                throw new Exception("No usable API apiKeyResult found.");
            }

            var apiKey = apiKeyResult.Value;

            var response = await _scraperApiClient.GetHtmlAsync(parameters);
            var actualCost = _scraperApiClient.GetCostFromResponse(response);

            if (response.IsSuccessStatusCode)
                apiKey.UpdateConsumedCredits(estimatedCost, actualCost);
            else
                apiKey.UpdateConsumedCredits(-estimatedCost);

            return response;
        }
    }
}