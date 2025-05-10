using HtmlAgilityPack;

namespace HallyuVault.Etl.FileNameExtractors
{
    public class PixeldrainFileNameExtractor : IFileNameExtractor
    {
        public string SupportedHost => "pixeldrain.com";
        private readonly HttpClient _httpClient;

        public PixeldrainFileNameExtractor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ExtractFileNameAsync(Uri url)
        {
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var contentDisposition = response.Content.Headers.ContentDisposition;
            
            if (contentDisposition == null || string.IsNullOrEmpty(contentDisposition.FileName))
            {
                throw new ArgumentException("Invalid URL: Unable to extract file name.");
            }

            var filename = contentDisposition.FileName.Trim('"');
            
            return filename;
        }
    }
}
