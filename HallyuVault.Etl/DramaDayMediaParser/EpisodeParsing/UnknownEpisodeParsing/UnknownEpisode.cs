using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.UnknownEpisodeParsing
{
    public class UnknownEpisode : Episode
    {
        public UnknownEpisode(string rawTitle) : base()
        {
            RawTitle = rawTitle;
        }

        public string RawTitle { get; private set; }
    }
}
