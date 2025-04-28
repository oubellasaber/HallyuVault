namespace HallyuVault.Etl.Domain.Entities.Episodes.StandardEpisodes;

public class StandardEpisode : Episode
{
    public StandardEpisode(EpisodeId id, int episodeNumber) : base(id)
    {
        EpisodeNumber = episodeNumber;
    }

    public int EpisodeNumber { get; private set; }
}