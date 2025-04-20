using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.HorizontalSeason
{
    public class HorizontalSeasonParser : HtmlNodeParser<Season>
    {
        public HorizontalSeasonParser(IHorizontalSeasonValidator validator) : base(validator)
        {
        }

        protected override Result<Season> ParseInternal(HtmlNode input)
        {
            var match = Regex.Match(
                input.SelectSingleNode("./td[1]").InnerText,
                @"season (\d+)",
                RegexOptions.IgnoreCase
            );

            var seasonNumber = match.Success ? int.Parse(match.Groups[1].Value) : 1;
            var season = new Season(seasonNumber);

            return season;
        }
    }
}
