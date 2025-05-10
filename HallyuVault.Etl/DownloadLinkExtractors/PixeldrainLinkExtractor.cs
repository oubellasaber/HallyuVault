
namespace HallyuVault.Etl.DownloadLinkExtractors
{
    public class PixeldrainLinkExtractor : IDownloadLinkExtractor
    {
        public string SupportedHost => "pixeldrain.com";

        public Task<Uri> ExtractDownloadLink(Uri url)
        {
            // Validate the URL format  
            if (url == null || url.Host != SupportedHost)
            {
                throw new ArgumentException("Invalid URL format. Expected a URL like https://send.tld/file_id.");
            }

            return Task.FromResult(new Uri($"https://pixeldrain.com/api/file/{url.Segments.Last()}?download"));
        }
    }
}
