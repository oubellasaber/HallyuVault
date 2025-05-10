using HallyuVault.Etl.DownloadLinkExtractors;
using HtmlAgilityPack;

namespace HallyuVault.Etl.FileNameExtractors
{
    public class AkiraBoxFileNameExtractor : IFileNameExtractor
    {
        public string SupportedHost => "akirabox.com";
        private readonly HttpClient _httpClient;
        private readonly CloudflareBypasser _cloudflareBypasser;

        public AkiraBoxFileNameExtractor(CloudflareBypasser cloudflareBypasser, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _cloudflareBypasser = cloudflareBypasser;
        }

        public async Task<string> ExtractFileNameAsync(Uri url)
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
            var response = await _httpClient.GetAsync("https://akirabox.com/downloads");
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extract the file name from the HTML content
            var fileNameNode = doc.DocumentNode.SelectSingleNode(@"//h1");

            if (fileNameNode is null)
            {
                throw new InvalidOperationException("Failed to extract file name from the HTML content.");
            }

            var fileName = fileNameNode.InnerText.Trim();

            return fileName;
        }
    }
}
