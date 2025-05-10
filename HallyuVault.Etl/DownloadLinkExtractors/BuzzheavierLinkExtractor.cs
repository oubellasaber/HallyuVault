namespace HallyuVault.Etl.DownloadLinkExtractors
{
    public class BuzzheavierLinkExtractor : IDownloadLinkExtractor
    {
        private readonly HttpClient _httpClient;

        public string SupportedHost => "buzzheavier.com";

        public BuzzheavierLinkExtractor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Uri> ExtractDownloadLink(Uri url)
        {
            // Validate the URL format  
            if (url == null || url.Host != SupportedHost)
            {
                throw new ArgumentException("Invalid URL format. Expected a URL like https://buzzheavier.com/id.");
            }

            // Extract the ID from the URL  
            var id = url.AbsolutePath.Trim('/');
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Invalid URL. Could not extract ID.");
            }

            // Construct the GET request  
            var requestUri = new Uri($"https://buzzheavier.com/{id}/download");
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");
            requestMessage.Headers.Add("referer", $"https://buzzheavier.com/{id}");

            // Send the request and get the response  
            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.Headers.TryGetValues("hx-redirect", out var redirectValues))
            {
                throw new InvalidOperationException("The response does not contain an 'hx-redirect' header, which is required to extract the download link.");
            }

            // Extract the redirect path and construct the full URI  
            var redirectPath = redirectValues.FirstOrDefault();
            if (string.IsNullOrEmpty(redirectPath))
            {
                throw new InvalidOperationException("The 'hx-redirect' header is empty.");
            }

            var downloadLink = new Uri(new Uri($"https://{url.Host}"), redirectPath);
            return downloadLink;
        }
    }
}
