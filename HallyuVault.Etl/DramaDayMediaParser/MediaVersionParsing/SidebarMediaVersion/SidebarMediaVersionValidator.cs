using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing.SidebarMediaVersion
{
    public class SidebarMediaVersionValidator : ISidebarMediaVersionValidator
    {
        public Result Validate(HtmlNode input)
        {
            var tdNodes = input.SelectNodes(".//td");

            var validationResult = Regex.IsMatch(tdNodes[0].InnerText, @"^\d{1,2}-\d{1,2}\s+(.+)$", RegexOptions.Singleline);

            if (!validationResult)
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
