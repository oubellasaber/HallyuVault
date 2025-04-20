using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.UnknownEpisodeParsing
{
    public class UnknownEpisodeParser : HtmlNodeParser<UnknownEpisode>, ISpecializedEpisodeParser<UnknownEpisode>
    {
        public UnknownEpisodeParser(UnknownEpisodeValidator validator) : base(validator)
        {
        }

        protected override Result<UnknownEpisode> ParseInternal(HtmlNode input)
        {
            var rawTitle = input.SelectSingleNode("./td[1]").InnerText;
            var unknownEpisode = new UnknownEpisode(rawTitle);

            return unknownEpisode;
        }
    }
}
