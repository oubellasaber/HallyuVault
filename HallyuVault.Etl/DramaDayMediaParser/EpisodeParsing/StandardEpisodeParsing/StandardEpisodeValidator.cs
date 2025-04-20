using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.StandardEpisodeParsing
{
    public class StandardEpisodeValidator : IStandardEpisodeValidator
    {
        public Result Validate(HtmlNode input)
        {
            var isSingle = Regex.IsMatch(
                input.SelectSingleNode("./td[1]").InnerText,
                @"^\s*\d{1,2}\s*(?:\n.*)?$",
                RegexOptions.Singleline);

            if (!isSingle)
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
