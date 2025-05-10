using System.Net;
using System.Text.Json;

namespace HallyuVault.Etl.DownloadLinkExtractors
{
    public class DatanodesLinkExtractor : IDownloadLinkExtractor
    {
        private readonly HttpClient _httpClient;

        public string SupportedHost => "datanodes.to";

        public DatanodesLinkExtractor(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<Uri> ExtractDownloadLink(Uri url)
        {
            // Validate the URL format
            if (url == null || url.Host != SupportedHost)
            {
                throw new ArgumentException("Invalid URL format. Expected a URL like https://datanodes.to/file_id.");
            }

            // Extract the ID from the URL
            var id = url.AbsolutePath.Trim('/');
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Invalid URL. Could not extract ID.");
            }

            var downloadLink = await GetDownloadLinkAsync(id);

            return downloadLink;
        }

        private async Task<Uri> GetDownloadLinkAsync(string id)
        {
            var requestUri = "https://datanodes.to/download";

            // Prepare the form data
            var formData = new Dictionary<string, string>
                   {
                       { "op", "download2" },
                       { "id", id },
                       { "dl", "1" }
                   };

            var content = new FormUrlEncodedContent(formData);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            // Create the POST request
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content
            };
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");

            // Send the request and get the response
            var response = await _httpClient.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to retrieve the download link. HTTP Status: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }

            // Parse the JSON response to extract the download link
            var json = await response.Content.ReadAsStringAsync();
            return ParseDownloadLinkFromJson(json);
        }

        private Uri ParseDownloadLinkFromJson(string jsonString)
        {
            try
            {
                using (JsonDocument document = JsonDocument.Parse(jsonString))
                {
                    if (document.RootElement.TryGetProperty("url", out JsonElement urlElement))
                    {
                        string encodedUrl = urlElement.ToString();
                        string decodedUrl = WebUtility.UrlDecode(encodedUrl);
                        string cleanedUrl = decodedUrl.Replace(["\n", "]", "["], ["", "%5D", "%5B"]);
                        return new Uri(cleanedUrl);
                    }
                    else
                    {
                        throw new InvalidOperationException("The 'url' property was not found in the JSON response.");
                    }
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to parse the JSON response.", ex);
            }
            catch (UriFormatException ex)
            {
                throw new InvalidOperationException("Failed to create a valid URI from the JSON response.", ex);
            }
        }
    }
}
