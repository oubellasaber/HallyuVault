using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.SidebarSeason
{
    public class SidebarSeasonParser : HtmlNodeParser<Season>
    {
        public SidebarSeasonParser(ISidebarSeasonValidator validator) : base(validator)
        {
        }

        protected override Result<Season> ParseInternal(HtmlNode input)
        {
            var seasonNumber = int.Parse(
                Regex.Match(
                    input.SelectSingleNode("./td[1]").InnerText,
                    @"s[\w\s]*(\d{1,2})",
                    RegexOptions.IgnoreCase)
                .Groups[1].Value);

            var season = new Season(seasonNumber);

            return season;
        }
    }
}
