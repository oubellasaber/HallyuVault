using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Core;
using Microsoft.Extensions.Options;

namespace HallyuVault.Etl.ApiKeyRotator.ScraperApi
{
    public class ScraperApiKeyManager : ApiKeyManager<ScraperApiKey, int>
    {
        public ScraperApiKeyManager(
            IApiKeyFactory<ScraperApiKey> apiKeyFactory,
            IOptionsMonitor<ApiKeyRotationOptions> optionsMonitor) :
            base(apiKeyFactory, optionsMonitor)
        {
        }

        protected override ScraperApiKey? SelectKey(int estimitedCredits)
        {
            var selectedKey = ApiKeys.Where(k => k.ConcurrentRequests < k.ConcurrencyLimit && k.AvailableCredits >= estimitedCredits)
                .OrderByDescending(k => k.AvailableCredits)
                .ThenByDescending(k => k.ConcurrentRequests)
                .FirstOrDefault();

            return selectedKey;
        }
    }
}