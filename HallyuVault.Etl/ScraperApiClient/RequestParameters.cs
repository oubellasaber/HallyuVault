namespace HallyuVault.Etl.ScraperApiClient
{
    public class RequestParameters
    {
        public string ApiKey { get; set; }
        public Uri Url { get; set; }
        public bool RenderJavaScript { get; set; }
        public bool Premium { get; set; }

        public RequestParameters()
        {
        }

        public RequestParameters(string apiKey, Uri url, bool renderJavaScript, bool premium)
        {
            ApiKey = apiKey;
            Url = url;
            RenderJavaScript = renderJavaScript;
            Premium = premium;
        }

        public RequestParameters(string apiKey, Uri url, bool premium)
        {
            ApiKey = apiKey;
            Url = url;
            Premium = premium;
        }
    }
}
