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

        public async Task<HttpResponseMessage> GetAsync(RequestParameters parameters)
        {
            int estimatedCost = _scraperApiClient.GetEstimatedCreditCost(parameters);
            var apiKeyResult = _scraperApiKeyManager.Get(estimatedCost);

            if (apiKeyResult.IsFailure)
            {
                throw new Exception("No usable API apiKeyResult found.");
            }

            var apiKey = apiKeyResult.Value;

            var response = await _scraperApiClient.GetAsync(parameters);
            var actualCost = _scraperApiClient.GetCostFromResponse(response);

            if (response.IsSuccessStatusCode)
                apiKey.UpdateConsumedCredits(estimatedCost, actualCost);
            else
                apiKey.UpdateConsumedCredits(-estimatedCost);

            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(RequestParameters parameters, StringContent content)
        {
            int estimatedCost = _scraperApiClient.GetEstimatedCreditCost(parameters);
            var apiKeyResult = _scraperApiKeyManager.Get(estimatedCost);

            if (apiKeyResult.IsFailure)
            {
                throw new Exception("No usable API apiKeyResult found.");
            }

            var apiKey = apiKeyResult.Value;
            parameters.ApiKey = apiKey.Key;

            var response = await _scraperApiClient.PostAsync(parameters, content);
            var actualCost = _scraperApiClient.GetCostFromResponse(response);

            if (response.IsSuccessStatusCode)
                apiKey.UpdateConsumedCredits(estimatedCost, actualCost);
            else
                apiKey.UpdateConsumedCredits(-estimatedCost);

            return response;
        }
    }
}