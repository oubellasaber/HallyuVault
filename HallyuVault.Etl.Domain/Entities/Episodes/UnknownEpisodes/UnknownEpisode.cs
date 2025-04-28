namespace HallyuVault.Etl.Domain.Entities.Episodes.UnknownEpisodes;

public class UnknownEpisode : Episode
{
    public UnknownEpisode(EpisodeId id, string rawTitle) : base(id)
    {
        RawTitle = rawTitle;
    }

    public string RawTitle { get; private set; }
}