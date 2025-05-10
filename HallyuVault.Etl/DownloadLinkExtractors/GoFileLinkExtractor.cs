using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HallyuVault.Etl.DownloadLinkExtractors
{
    public class GoFileLinkExtractor : IDownloadLinkExtractor
    {
        private readonly string _accessToken;
        private readonly HttpClient _httpClient;
        public string SupportedHost => "gofile.io";

        public GoFileLinkExtractor(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _accessToken = configuration["GoFileOptions:AccessToken"]
                           ?? throw new InvalidOperationException("Access token is not configured.");
        }

        public async Task<Uri> ExtractDownloadLink(Uri url)
        {
            // Validate the URL format  
            if (url == null || url.Host != SupportedHost)
            {
                throw new ArgumentException("Invalid URL format. Expected a URL like https://gofile.io/d/file_id.");
            }

            // Extract the ID from the URL  
            var id = url.Segments.LastOrDefault()?.Trim('/');
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Invalid URL. Could not extract ID.");
            }

            var requestUrl = $"https://api.gofile.io/contents/{id}?wt=4fd6sg89d7s6&contentFilter=&page=1&pageSize=1000&sortField=name&sortDirection=1";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            // Add authorization header  
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse JSON response  
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var root = jsonDoc.RootElement;

            // Extract the "link" field from the response  
            var link = root.GetProperty("data")
                           .GetProperty("children")
                           .EnumerateObject()
                           .FirstOrDefault()
                           .Value
                           .GetProperty("link")
                           .GetString();

            if (string.IsNullOrEmpty(link))
            {
                throw new InvalidOperationException("Download link not found in the response.");
            }

            return new Uri(link);
        }
    }
}