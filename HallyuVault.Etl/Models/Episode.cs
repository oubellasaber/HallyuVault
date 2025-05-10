using System.ComponentModel.DataAnnotations.Schema;

namespace HallyuVault.Etl.Models
{
    public class Episode
    {
        private readonly List<EpisodeVersion> _versions = new();

        public int Id { get; set; }
        public IReadOnlyCollection<EpisodeVersion> EpisodeVersions => _versions.AsReadOnly();

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
