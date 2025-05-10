using HallyuVault.Etl.ApiKeyRotator.Core;

namespace HallyuVault.Etl.ApiKeyRotator.ScraperApi
{
    public class ScraperApiKey : ApiKey
    {
        private string _apiKey;
        public int TotalCredits { get; }
        public int ConsumedCredits { get; private set; }
        public int ConcurrencyLimit { get; }
        public int ConcurrentRequests { get; private set; }
        public DateTime SubscriptionDate { get; }

        public int AvailableCredits => TotalCredits - ConsumedCredits;
        public override string Key => _apiKey;

        public void UpdateConsumedCredits(int by)
        {
            if (by > 0)
            {
                ConsumedCredits += by;
                ++ConcurrentRequests;
            }
            else
            {
                ConsumedCredits -= by;
                --ConcurrentRequests;
            }
        }

        public void UpdateConsumedCredits(int dec, int inc)
        {
            ConsumedCredits = ConsumedCredits - dec + inc;
            --ConcurrentRequests;
        }

        public ScraperApiKey(
            string apiKey,
            int totalCredits,
            int consumedCredits,
            int concurrencyLimit,
            int concurrentRequests,
            DateTime subscriptionDate)
        {
            _apiKey = apiKey;
            TotalCredits = totalCredits;
            ConsumedCredits = consumedCredits;
            ConcurrencyLimit = concurrencyLimit;
            ConcurrentRequests = concurrentRequests;
            SubscriptionDate = subscriptionDate;
        }
    }
}