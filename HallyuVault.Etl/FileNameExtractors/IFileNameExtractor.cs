namespace HallyuVault.Etl.FileNameExtractors
{
    public interface IFileNameExtractor
    {
        /// <summary>
        /// Extracts the file name from the given path.
        /// </summary>
        /// <param name="path">The url to extract the file name from.</param>
        /// <returns>The extracted file name.</returns>
        Task<string> ExtractFileNameAsync(Uri url);

        public string SupportedHost { get; }
    }
}
