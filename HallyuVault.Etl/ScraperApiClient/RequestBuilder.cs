namespace HallyuVault.Etl.ScraperApiClient
{
    public class RequestBuilder
    {
        private readonly ScraperApiClient _client;
        private readonly RequestParameters _options = new RequestParameters();

        public RequestBuilder(ScraperApiClient client)
        {
            _client = client;
        }

        public RequestBuilder ForUrl(string url)
        {
            _options.Url = url;
            return this;
        }

        public RequestBuilder WithJavaScript()
        {
            _options.RenderJavaScript = true;
            return this;
        }

        public RequestBuilder WithPremium()
        {
            _options.Premium = true;
            return this;
        }

        // Additional builder methods...

        public Task<HttpResponseMessage> GetHtmlAsync()
        {
            return _client.GetHtmlAsync(_options);
        }
    }
}
