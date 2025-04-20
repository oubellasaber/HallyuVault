using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing.HorizontalMediaVersion
{
    internal class HorizontalMediaVersionValidator : IHorizontalMediaVersionValidator
    {
        public Result Validate(HtmlNode input)
        {
            var tdNodes = input.SelectNodes(".//td")?.ToList();

            var validationResult = tdNodes?.Count switch
            {
                3 => ValidateThreeCellVersion(tdNodes),
                2 => ValidateTwoCellVersion(tdNodes),
                _ => false
            };

            if (!validationResult)
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }

        private static bool ValidateThreeCellVersion(IReadOnlyList<HtmlNode> tdNodes)
        {
            return !string.IsNullOrWhiteSpace(tdNodes[0].InnerText) && // First cell has content
                   string.IsNullOrWhiteSpace(tdNodes[1].InnerText) &&  // Second cell is empty
                   string.IsNullOrWhiteSpace(tdNodes[2].InnerText);    // Third cell is empty
        }

        private static bool ValidateTwoCellVersion(IReadOnlyList<HtmlNode> tdNodes)
        {
            return string.IsNullOrWhiteSpace(tdNodes[0].InnerText) && // First cell is empty
                   !string.IsNullOrWhiteSpace(tdNodes[1].InnerText);  // Second cell has content
        }
    }
}
