using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.UnknownEpisodeParsing
{
    public class UnknownEpisodeParser : HtmlNodeParser<Episode>
    {
        public UnknownEpisodeParser(IUnknownEpisodeValidator validator) : base(validator)
        {
        }

        protected override Result<Episode> ParseInternal(HtmlNode input)
        {
            var rawTitle = input.SelectSingleNode("./td[1]").InnerText;
            var unknownEpisode = new UnknownEpisode(rawTitle);

            return unknownEpisode;
        }
    }
}
