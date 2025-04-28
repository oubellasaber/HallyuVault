using HallyuVault.Core.Abstractions;

namespace HallyuVault.Etl.Domain.Entities.Episodes.SpecialEpisodes;

public class SpecialEpisode : Episode
{
    public SpecialEpisode(EpisodeId id, string title) : base(id)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(title));

        Title = title;
    }

    public string Title { get; private set; }
}