using HallyuVault.Etl.DownloadLinkExtractors;
using System.Net;

namespace HallyuVault.Etl.FileNameExtractors
{
    internal class SendCmFileNameExtractor : IFileNameExtractor
    {
        public string SupportedHost => "send.cm";
        private readonly SendCmLinkExtractor _sendCmLinkExtractor;
        public SendCmFileNameExtractor(SendCmLinkExtractor sendCmLinkExtractor)
        {
            _sendCmLinkExtractor = sendCmLinkExtractor;
        }

        public async Task<string> ExtractFileNameAsync(Uri url)
        {
            // Extract the file name from the URL using the SendCmFileNameExtractor
            var downloadLink = await _sendCmLinkExtractor.ExtractDownloadLink(url);
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
