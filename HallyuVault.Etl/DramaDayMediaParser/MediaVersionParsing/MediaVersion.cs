using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;

namespace HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing
{
    public class MediaVersion
    {
        private readonly List<Episode> _episodes = new();

        public MediaVersion(string name)
        {
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyCollection<Episode> Episodes => _episodes.AsReadOnly();

        public void AddEpisode(Episode episode)
        {
            _episodes.Add(episode);
        }

        public static readonly MediaVersion Default = new MediaVersion("Default");
    }
}
