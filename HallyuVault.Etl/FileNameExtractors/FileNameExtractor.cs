namespace HallyuVault.Etl.FileNameExtractors
{
    public class FileNameExtractor
    {
        private readonly IEnumerable<IFileNameExtractor> _extractors;

        public FileNameExtractor(IEnumerable<IFileNameExtractor> extractors)
            => _extractors = extractors;

        public async Task<string> ExtractFileNameAsync(Uri url)
        {
            var extractor = _extractors.FirstOrDefault(d => d.SupportedHost == url.Host);
            var isSupported = extractor != null;

            if (!isSupported)
            {
                throw new NotSupportedException($"The URL host '{url.Host}' is not supported.");
            }

            var link = await extractor.ExtractFileNameAsync(url);

            return link;
        }
    }
}
