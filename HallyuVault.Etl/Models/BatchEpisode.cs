namespace HallyuVault.Etl.Models
{
    public class BatchEpisode : Episode
    {
        public BatchEpisode(Range episodeRange)
        {
            EpisodeRange = episodeRange;
        }

        public Range EpisodeRange { get; private set; }
    }
}
