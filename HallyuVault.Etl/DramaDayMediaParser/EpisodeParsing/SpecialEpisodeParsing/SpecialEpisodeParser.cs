using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.SpecialEpisodeParsing
{
    internal class SpecialEpisodeParser : HtmlNodeParser<SpecialEpisode>, ISpecializedEpisodeParser<SpecialEpisode>
    {
        public SpecialEpisodeParser(ISpecialEpisodeValidator validator) : base(validator)
        {
        }

        protected override Result<SpecialEpisode> ParseInternal(HtmlNode input)
        {
            var title = input.SelectSingleNode("./td[1]").InnerText;
            var specialEpisode = new SpecialEpisode(title);

            return specialEpisode;
        }
    }
}
