using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.SpecialEpisodeParsing
{
    public class SpecialEpisodeParser : HtmlNodeParser<Episode>
    {
        public SpecialEpisodeParser(ISpecialEpisodeValidator validator) : base(validator)
        {
        }

        protected override Result<Episode> ParseInternal(HtmlNode input)
        {
            var title = input.SelectSingleNode("./td[1]").InnerText;
            var specialEpisode = new SpecialEpisode(title);

            return specialEpisode;
        }
    }
}
