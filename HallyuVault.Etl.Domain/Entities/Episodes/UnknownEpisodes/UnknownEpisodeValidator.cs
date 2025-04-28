using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.Domain.Entities.Episodes.UnknownEpisodes;

public class UnknownEpisodeValidator : IHtmlNodeValidator
{
    public Result Validate(HtmlNode input)
    {
        if (!string.IsNullOrEmpty(input.SelectSingleNode("./td[1]").InnerText))
            return Result.Failure(HtmlParsingErrors.MismatchedParser);

        return Result.Success();
    }
}