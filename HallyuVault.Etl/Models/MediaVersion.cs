using System.ComponentModel.DataAnnotations.Schema;

namespace HallyuVault.Etl.Models
{
    public class MediaVersion
    {
        private readonly List<Episode> _episodes = new();

        private MediaVersion() { }

        public MediaVersion(string name)
        {
            Name = name;
        }

        public int Id { get; set; }
        public string Name { get; private set; }
        public IReadOnlyCollection<Episode> Episodes => _episodes.AsReadOnly();

        public void AddEpisode(Episode episode)
        {
            _episodes.Add(episode);
        }

        public static MediaVersion Default => new MediaVersion("Default");
    }
}
