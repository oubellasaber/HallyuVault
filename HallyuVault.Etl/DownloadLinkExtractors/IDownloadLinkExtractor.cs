namespace HallyuVault.Etl.DownloadLinkExtractors;

public interface IDownloadLinkExtractor
{
    /// <summary>
    /// Extracts the download link from the given URL.
    /// </summary>
    /// <param name="url">The URI to extract the download link from.</param>
    /// <returns>The extracted download link.</returns>
    Task<Uri> ExtractDownloadLink(Uri url);

    /// <summary>
    /// Gets the host that this extractor supports.
    /// </summary>
    string SupportedHost { get; }
}
