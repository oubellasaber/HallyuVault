using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.UrlSeason
{
    public class UrlSeasonParser : HtmlNodeParser<Season>
    {
        public UrlSeasonParser(IUrlSeasonValidator validator) : base(validator)
        {
        }

        protected override Result<Season> ParseInternal(HtmlNode input)
        {
            var urlMetaNode = input.OwnerDocument.DocumentNode.SelectSingleNode(@"//meta[@property = ""og:url""]");
            var url = urlMetaNode.GetAttributeValue("content", string.Empty);
            var seasonNumber = GetDramaSeason(url);
            var season = new Season(seasonNumber);

            return season;
        }

        private static int GetDramaSeason(string url)
        {
            int season = 1;

            string[] titleParts = url.Split("-");
            string lastPart = titleParts[titleParts.Length - 1].Trim('/');

            if (lastPart.Length == 1 &&
                int.TryParse(lastPart, out int result) &&
                result > 1 && result < 7)
            {
                season = result;
            }

            return season;
        }
    }
}
