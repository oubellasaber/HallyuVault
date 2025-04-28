using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing
{
    public static class EpisodeVersionParsingUtility
    {
        public static string[] ExtractLinkGroups(string cellHtml)
        {
            return cellHtml.Split("<br>", StringSplitOptions.RemoveEmptyEntries);
        }

        public static ICollection<ICollection<DramaDayLink>> ParseLinks(string cellHtml)
        {
            var rawLinkGroups = ExtractLinkGroups(cellHtml);
            var scrapedLinkGroups = new List<ICollection<DramaDayLink>>();

            foreach (var rawLinkGroup in rawLinkGroups)
                scrapedLinkGroups.Add(ExtractLinksFromGroup(rawLinkGroup));

            return scrapedLinkGroups;
        }

        public static ICollection<ICollection<DramaDayLink>> ParseLinks(ICollection<HtmlNode> cells)
        {
            var rawLinkGroups = cells.Select(c => c.InnerHtml).ToArray();
            var scrapedLinkGroups = new List<ICollection<DramaDayLink>>();

            foreach (var rawLinkGroup in rawLinkGroups)
                scrapedLinkGroups.Add(ExtractLinksFromGroup(rawLinkGroup));

            return scrapedLinkGroups;
        }

        private static ICollection<DramaDayLink> ExtractLinksFromGroup(string linkGroupHtml)
        {
            return HtmlNode.CreateNode($"<div>{linkGroupHtml}</div>")
                       .SelectNodes(".//a")
                       .Select(l => new DramaDayLink(l.InnerText.Trim(), new Uri(l.GetAttributeValue("href", ""))))
                       .ToList();
        }
    }
}
