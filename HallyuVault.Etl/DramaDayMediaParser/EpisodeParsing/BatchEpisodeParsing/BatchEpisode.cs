using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.BatchEpisodeParsing
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
