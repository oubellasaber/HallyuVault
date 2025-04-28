namespace HallyuVault.Etl.Models
{
    public class StandardEpisode : Episode
    {
        public StandardEpisode(int episodeNumber)
        {
            EpisodeNumber = episodeNumber;
        }

        public int EpisodeNumber { get; private set; }
    }
}