using HallyuVault.Etl.DownloadLinkExtractors;
using System.Net;

namespace HallyuVault.Etl.FileNameExtractors
{
    public class DatanodesFileNameExtractor : IFileNameExtractor
    {
        public string SupportedHost => "datanodes.to";

        private readonly DatanodesLinkExtractor _datanodesLinkExtractor;

        public DatanodesFileNameExtractor(DatanodesLinkExtractor datanodesLinkExtractor)
        {
            _datanodesLinkExtractor = datanodesLinkExtractor;
        }

        public async Task<string> ExtractFileNameAsync(Uri url)
        {
            // Extract the file name from the URL using the DatanodesLinkExtractor
            var downloadLink = await _datanodesLinkExtractor.ExtractDownloadLink(url);
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