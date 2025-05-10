using System.Text.Json;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DownloadLinkExtractors
{
    public class AkiraBoxLinkExtractor : IDownloadLinkExtractor
    {
        private readonly HttpClient _httpClient;
        private readonly CloudflareBypasser _cloudflareBypasser;
        public string SupportedHost => "akirabox.com";

        public AkiraBoxLinkExtractor(CloudflareBypasser cloudflareBypasser, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _cloudflareBypasser = cloudflareBypasser;
        }


        public async Task<Uri> ExtractDownloadLink(Uri url)
        {
            // Validate the URL format  
            if (url == null || url.Host != SupportedHost)
            {
                throw new ArgumentException("Invalid URL format. Expected a URL like https://akirabox.com/id/file.");
            }

            // Use CloudflareBypasser to get cookies and headers  
            var bypassResult = await _cloudflareBypasser.BypassAsync(url.ToString());
            if (bypassResult == null)
            {
                throw new InvalidOperationException("Failed to bypass Cloudflare protection.");
            }

            // Set cookies and headers for subsequent requests  
            var cookieHeader = string.Join("; ", bypassResult.Cookies.Select(c => $"{c.Name}={c.Value}"));
            _httpClient.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            foreach (var header in bypassResult.Headers)
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Fetch the CSRF token from the downloads page  
            var downloadsPageResponse = await _httpClient.GetAsync("https://akirabox.com/downloads");
            downloadsPageResponse.EnsureSuccessStatusCode();
            var downloadsPageHtml = await downloadsPageResponse.Content.ReadAsStringAsync();

            var csrfTokenMatch = Regex.Match(downloadsPageHtml, @"<meta name=""csrf-token"" content=""(.*?)"">");
            if (!csrfTokenMatch.Success)
            {
                throw new InvalidOperationException("Failed to extract CSRF token from the downloads page.");
            }
            var csrfToken = csrfTokenMatch.Groups[1].Value;

            // Prepare the POST request to generate the download link  
            var requestUri = new Uri($"{url.Scheme}://{url.Host}/{url.AbsolutePath.Trim('/')}/generate");
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Headers = { { "x-csrf-token", csrfToken } }
            };

            // Send the request and get the response  
            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            // Extract the download link from the response  
            var responseBody = await response.Content.ReadAsStringAsync();

            // Parse the response body as JSON to extract the download_url property
            using var jsonDocument = JsonDocument.Parse(responseBody);
            if (!jsonDocument.RootElement.TryGetProperty("download_link", out var downloadUrlElement))
            {
                throw new InvalidOperationException("Failed to extract the download link from the response.");
            }

            var downloadUrl = downloadUrlElement.GetString();
            if (string.IsNullOrEmpty(downloadUrl))
            {
                throw new InvalidOperationException("The download link is empty or null.");
            }

            return new Uri(downloadUrl);
        }
    }
}
