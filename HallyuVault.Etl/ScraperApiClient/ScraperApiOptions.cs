namespace HallyuVault.Etl.ScraperApiClient
{
    public class ScraperApiOptions
    {
        public string BaseUrl { get; set; }
        public int DefaultTimeout { get; set; }
        public string AccountEndpoint { get; set; }
        public string UrlCostEndpoint { get; set; }
    }
}