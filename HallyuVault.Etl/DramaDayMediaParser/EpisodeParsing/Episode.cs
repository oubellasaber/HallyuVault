using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing
{
    public abstract class Episode
    {
        private readonly List<EpisodeVersion> _versions = new();

        protected Episode() { }

        public int Id { get; private set; }
        public IReadOnlyCollection<EpisodeVersion> Versions => _versions.AsReadOnly();

        public void AddEpisodeVersion(EpisodeVersion version)
        {
            _versions.Add(version);
        }

        public void AddEpisodeVersionRange(IEnumerable<EpisodeVersion> versions)
        {
            foreach (var version in versions)
            {
                AddEpisodeVersion(version);
            }
        }
    }
}
