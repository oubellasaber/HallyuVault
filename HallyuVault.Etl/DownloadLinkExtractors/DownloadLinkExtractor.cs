
namespace HallyuVault.Etl.DownloadLinkExtractors
{
    public class DownloadLinkExtractor
    {
        private readonly IEnumerable<IDownloadLinkExtractor> _downloaders;

        public DownloadLinkExtractor(IEnumerable<IDownloadLinkExtractor> downloaders)
            => _downloaders = downloaders;

        public async Task<Uri> ExtractDownloadLink(Uri url)
        {
            var downloader = _downloaders.FirstOrDefault(d => d.SupportedHost == url.Host);
            var isSupported = downloader != null;

            if (!isSupported)
            {
                throw new NotSupportedException($"The URL host '{url.Host}' is not supported.");
            }

            var link = await downloader.ExtractDownloadLink(url);

            return link;
        }
    }
}
