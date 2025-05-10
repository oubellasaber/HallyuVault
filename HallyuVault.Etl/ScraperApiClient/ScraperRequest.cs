namespace HallyuVault.Etl.ScraperApiClient
{
    public class ScraperRequest
    {
        public string ApiKey { get; set; }
        public Uri Url { get; set; }
        public HttpMethod Method { get; set; } = HttpMethod.Get;
        public StringContent? Content { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
    }

}
