using HallyuVault.Etl.DownloadLinkExtractors;
using System.Net;

namespace HallyuVault.Etl.FileNameExtractors
{
    public class GoFileNameExtractor : IFileNameExtractor
    {
        public string SupportedHost => throw new NotImplementedException();
        private readonly GoFileLinkExtractor _goFileLinkExtractor;
        public GoFileNameExtractor(GoFileLinkExtractor gofileFilenameExtractor)
        {
            _goFileLinkExtractor = gofileFilenameExtractor;
        }

        public async Task<string> ExtractFileNameAsync(Uri url)
        {
            // Extract the file name from the URL using the GoFileLinkExtractor
            var downloadLink = await _goFileLinkExtractor.ExtractDownloadLink(url);
            var rawFilename = downloadLink.Segments.LastOrDefault();

            if (string.IsNullOrEmpty(rawFilename))
            {
                throw new ArgumentException("Invalid URL: Unable to extract file name.");
            }

            // Decode the URL-encoded file name
            var decodedFileName = WebUtility.UrlDecode(rawFilename);

            // Return the extracted file name
            return decodedFileName;
        }
    }
}