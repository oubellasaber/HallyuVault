using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.SidebarSeason
{
    internal class SidebarSeasonValidator : ISidebarSeasonValidator
    {
        public Result Validate(HtmlNode input)
        {
            var tdNodes = input.SelectNodes(".//td");

            var firstCellText = tdNodes[0].InnerText;
            if (!Regex.IsMatch(firstCellText, @"(s[\w\s]{0, 3}|season*)(\d{1,2})", RegexOptions.IgnoreCase))
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
