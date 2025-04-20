using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.HorizontalSeason
{
    public class HorizontalSeasonValidator : IHorizontalSeasonValidator
    {
        public Result Validate(HtmlNode input)
        {
            var tdNodes = input.SelectNodes(".//td");
            var firstCellText = tdNodes[0].InnerText;

            if (!(Regex.IsMatch(firstCellText, @"season (\d+)", RegexOptions.IgnoreCase) &&
                string.IsNullOrEmpty(tdNodes[1].InnerText)))
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
