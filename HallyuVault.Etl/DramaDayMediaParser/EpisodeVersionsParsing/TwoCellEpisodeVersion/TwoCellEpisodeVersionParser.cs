using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.TwoCellEpisodeVersion
{
    internal class TwoCellEpisodeVersionParser : EpisodeVersionParser, ISpecializedEpisodeVersionParser
    {
        public TwoCellEpisodeVersionParser(ITwoCellEpisodeVersionValidator validator) : base(validator)
        {
        }

        protected override IEnumerable<IEnumerable<DramaDayLink>> ParseLinks(HtmlNode input)
        {
            var links = EpisodeVersionParsingUtility.ParseLinks(input.SelectNodes(".//td")[1].InnerHtml);

            return links;
        }

        protected override IEnumerable<string> ParseQualities(HtmlNode input)
        {
            var qualityGroups = new List<string>();
            var textNodes = input.SelectNodes(".//td")[1].SelectNodes(".//text()");

            if (textNodes != null)
            {
                foreach (var textNode in textNodes.ToList())
                {
                    if (textNode.InnerText.Contains(":"))
                    {
                        qualityGroups.Add(textNode.InnerText.Replace(":", "").Trim());
                        textNode.Remove();
                    }
                }
            }

            return qualityGroups;
        }
    }
}
