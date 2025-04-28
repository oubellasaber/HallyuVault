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

        public RequestBuilder ForUrl(Uri url)
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

        public Task<HttpResponseMessage> GetAsync()
        {
            return _client.GetAsync(_options);
        }

        public Task<HttpResponseMessage> PostAsync(StringContent content)
        {
            return _client.PostAsync(_options, content);
        }
    }
}
