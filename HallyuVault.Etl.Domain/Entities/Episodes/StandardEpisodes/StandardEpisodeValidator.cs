using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.Domain.Entities.Episodes.StandardEpisodes;

public class StandardEpisodeValidator : IHtmlNodeValidator
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