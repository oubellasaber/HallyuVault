using HallyuVault.Etl.Domain.Abstractions;

namespace HallyuVault.Etl.Domain.Entities.Episodes;

public abstract class Episode : Entity<EpisodeId>
{
    public Episode(EpisodeId episodeId) : base(episodeId)
    {
    }

    private readonly List<EpisodeVersion> _versions = new();

    public IReadOnlyCollection<EpisodeVersion> EpisodeVersions => _versions.AsReadOnly();

    public void AddEpisodeVersion(EpisodeVersion version)
    {
        _versions.Add(version);
    }

    public void AddEpisodeVersions(IEnumerable<EpisodeVersion> versions)
    {
        foreach (var version in versions)
        {
            AddEpisodeVersion(version);
        }
    }
}