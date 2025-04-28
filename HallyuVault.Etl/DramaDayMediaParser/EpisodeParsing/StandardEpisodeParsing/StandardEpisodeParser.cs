using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.StandardEpisodeParsing
{
    public class StandardEpisodeParser : HtmlNodeParser<StandardEpisode>, ISpecializedEpisodeParser<StandardEpisode>
    {
        public StandardEpisodeParser(IStandardEpisodeValidator validator) : base(validator)
        {
        }

        protected override Result<StandardEpisode> ParseInternal(HtmlNode input)
        {
            var episodeNumber = int.Parse(
                    input.SelectSingleNode("./td[1]")
                    .InnerText
                    .Substring(0, Math.Min(2, input.SelectSingleNode("./td[1]").InnerText.Length))
                );

            var standardEpisode = new StandardEpisode(episodeNumber);

            return standardEpisode;
        }
    }
}
