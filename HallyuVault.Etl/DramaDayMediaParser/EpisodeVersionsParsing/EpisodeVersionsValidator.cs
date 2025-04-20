using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing
{
    public class EpisodeVersionsValidator : IEpisodeVersionsValidator
    {
        public Result Validate(HtmlNode input)
        {
            var tdNodes = input.SelectNodes(".//td");
            bool isValid = false;

            if (tdNodes.Count == 2)
            {
                var processedHtml = tdNodes[1].InnerHtml;

                // Regex for qualities
                isValid = Regex.IsMatch(tdNodes[1].InnerHtml,
                    @"(.+:)\s*((?:.*?\|\s*)*)",
                    RegexOptions.Singleline);
            }

            if (tdNodes.Count == 3)
            {
                // Regex for qualities
                isValid = Regex.IsMatch(tdNodes[1].InnerHtml, "^(?!.*<br>$).*$") &&
                    tdNodes[2].SelectSingleNode("./a") != null;

                // Regex for links
                isValid &= Regex.IsMatch(tdNodes[2].InnerText, @"((?:[\w\s]+(?:\s*\|\s*)?)+)") && tdNodes[2].SelectNodes(".//a") != null;
            }

            if (!isValid)
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
