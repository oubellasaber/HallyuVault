namespace HallyuVault.Etl.ScraperApiClient
{
    public class ScraperApiOptions
    {
        public string BaseUrl { get; private set; } = "http://api.scraperapi.com";
        public int DefaultTimeout { get; private set; } = 60000; // 60 seconds
        public string AccountEndpoint { get; private set; } = "/account";
        public string UrlCostEndpoint => $"{AccountEndpoint}/urlcost";
    }
}
