using System.Net;

namespace HallyuVault.Etl.DownloadLinkExtractors;

public class SendCmLinkExtractor : IDownloadLinkExtractor
{
    private readonly HttpClient _httpClient;
    public string SupportedHost => "send.now";

    public SendCmLinkExtractor(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Uri> ExtractDownloadLink(Uri url)
    {
        // Validate the URL format  
        if (url == null || url.Host != SupportedHost)
        {
            throw new ArgumentException("Invalid URL format. Expected a URL like https://send.tld/file_id.");
        }

        // Extract the ID from the URL  
        var id = url.AbsolutePath.Trim('/');
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Invalid URL. Could not extract ID.");
        }

        // Construct the POST request  
        var requestUri = new Uri($"https://send.now/{id}");
        var requestContent = new FormUrlEncodedContent(new[]
        {
              new KeyValuePair<string, string>("op", "download2"),
              new KeyValuePair<string, string>("id", id)
          });

        // Set the User-Agent header  
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = requestContent,
        };
        requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");

        // Send the request and get the response  
        var response = await _httpClient.SendAsync(requestMessage);
        if (response.StatusCode != HttpStatusCode.Redirect)
        {
            throw new HttpRequestException($"Expected to get a redirection. Received unexpected status code: {response.StatusCode}. Ensure the provided URL is correct and the server is accessible.");
        }

        // Extract the "Location" header from the response  
        if (response.Headers.Location == null)
        {
            throw new InvalidOperationException("The response does not contain a 'Location' header, which is required to extract the download link.");
        }

        // Return the extracted URI  
        var downloadLink = response.Headers.Location;

        // Contruct the prefered download link
        var preferredDownloadLink = new Uri(downloadLink.ToString().Replace(downloadLink.Host.Split(".")[1], "filescdn"));

        return preferredDownloadLink;
    }
}
