using Microsoft.Extensions.Options;

namespace HallyuVault.Etl.ScraperApiClient
{
    public class RequestBuilder
    {
        private readonly ScraperApiOptions _options;
        public RequestParameters Parameters { get; private set; } = new();

        public RequestBuilder(IOptions<ScraperApiOptions> options)
        {
            _options = options.Value;
        }

        public RequestBuilder WithApiKey(string apiKey)
        {
            Parameters.ApiKey = apiKey;
            return this;
        }

        public RequestBuilder ForUrl(Uri url)
        {
            Parameters.Url = url;
            return this;
        }

        public RequestBuilder WithJavaScript()
        {
            Parameters.RenderJavaScript = true;
            return this;
        }

        public RequestBuilder WithPremium()
        {
            Parameters.Premium = true;
            return this;
        }

        public ScraperRequest Build(HttpMethod method, StringContent? content = null)
        {
            string fullUrl = BuildRequestUrl(Parameters);
            return new ScraperRequest
            {
                Url = new Uri(fullUrl),
                Method = method,
                Content = content
            };
        }

        private string BuildRequestUrl(RequestParameters parameters)
        {
            var queryParams = new Dictionary<string, string>
            {
                ["api_key"] = parameters.ApiKey,
                ["url"] = parameters.Url.ToString()
            };

            if (parameters.RenderJavaScript) queryParams["render"] = "true";
            if (parameters.Premium) queryParams["premium"] = "true";

            var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            return $"{_options.BaseUrl}?{queryString}";
        }
    }

}
