using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.SpecialEpisodeParsing
{
    public class SpecialEpisode : Episode
    {
        public SpecialEpisode(string title)
        {
            Title = title;
        }

        public string Title { get; private set; }
    }
}
