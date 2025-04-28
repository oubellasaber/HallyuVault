using HallyuVault.Etl.Domain.Abstractions;
using HallyuVault.Etl.Domain.Entities.DramaDayLinks;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.NoTableEpisodeVersion
{
    public interface INoTableEpisodeVersionValidator : IHtmlNodeValidator;
    public class NoTableEpisodeVersionParser : EpisodeVersionParser, ISpecializedEpisodeVersionParser
    {
        public NoTableEpisodeVersionParser(INoTableEpisodeVersionValidator validator) : base(validator)
        {
        }

        protected override IEnumerable<IEnumerable<DramaDayLink>> ParseLinks(HtmlNode input)
        {
            var links = EpisodeVersionParsingUtility.ParseLinks(input.SelectNodes(".//td"));

            return links;
        }

        protected override IEnumerable<string> ParseQualities(HtmlNode input)
        {
            var qualities = input.SelectNodes(".//td")
                .Select(c => c.SelectSingleNode("./strong").InnerText);

            return qualities;
        }
    }
}