namespace HallyuVault.Etl.Models
{
    public class BatchEpisode : Episode
    {
        public BatchEpisode(int episodeStart, int episodeEnd)
        {
            if (episodeStart > episodeEnd)
                throw new ArgumentException("Episode end must be greater than episode start");

            if (episodeStart < 0 || episodeEnd < 0)
                throw new ArgumentException("Episode must be non negative");

            EpisodeStart = episodeStart;
            EpisodeEnd = episodeEnd;
        }

        public int EpisodeStart { get; private set; }
        public int EpisodeEnd { get; private set; }
    }
}
