using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.StandardEpisodeParsing
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