using HallyuVault.Etl.Models;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing
{
    public class Season
    {
        private readonly List<MediaVersion> _mediaVersions = new();

        public Season(int? seasonNumber)
        {
            SeasonNumber = seasonNumber;
        }

        public int Id { get; private set; }
        public int? SeasonNumber { get; private set; }
        public IReadOnlyCollection<MediaVersion> MediaVersions => _mediaVersions.AsReadOnly();

        public void AddMediaVersion(MediaVersion mediaVersion)
        {
            _mediaVersions.Add(mediaVersion);
        }
    }
}
