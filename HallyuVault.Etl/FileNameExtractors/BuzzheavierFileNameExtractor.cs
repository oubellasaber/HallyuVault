using HtmlAgilityPack;

namespace HallyuVault.Etl.FileNameExtractors
{
    public class BuzzheavierFileNameExtractor : IFileNameExtractor
    {
        public string SupportedHost => "buzzheavier.com";
        private readonly HttpClient _httpClient;

        public BuzzheavierFileNameExtractor(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ExtractFileNameAsync(Uri url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var titleNode = doc.DocumentNode.SelectSingleNode("//title");

            if (titleNode == null)
            {
                throw new ArgumentException("Invalid Html: Unable to extract file name.");
            }

            var filename = titleNode.InnerText;

            return filename;
        }
    }
}
