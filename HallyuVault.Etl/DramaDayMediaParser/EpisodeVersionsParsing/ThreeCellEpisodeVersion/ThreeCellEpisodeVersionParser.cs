using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.ThreeCellEpisodeVersion
{
    public class ThreeCellEpisodeVersionParser : EpisodeVersionParser, ISpecializedEpisodeVersionParser
    {
        public ThreeCellEpisodeVersionParser(IThreeCellEpisodeVersionValidator validator) : base(validator)
        {
        }

        protected override IEnumerable<IEnumerable<DramaDayLink>> ParseLinks(HtmlNode input)
        {
            var links = EpisodeVersionParsingUtility.ParseLinks(input.SelectNodes(".//td")[2].InnerHtml);

            return links;
        }

        protected override IEnumerable<string> ParseQualities(HtmlNode input)
        {
            var qualities = input.SelectNodes(".//td")[1]
                .InnerHtml
                .Split("<br>", StringSplitOptions.RemoveEmptyEntries);

            return qualities;
        }
    }
}
