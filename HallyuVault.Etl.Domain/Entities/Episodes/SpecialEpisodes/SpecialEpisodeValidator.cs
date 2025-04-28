using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.Domain.Entities.Episodes.SpecialEpisodes;

public class SpecialEpisodeValidator : IHtmlNodeValidator
{
    public Result Validate(HtmlNode input)
    {
        var isSpecial = input.SelectSingleNode("./td[1]")
               .InnerText
               .Contains("special", StringComparison.OrdinalIgnoreCase);

        if (!isSpecial)
            return Result.Failure(HtmlParsingErrors.MismatchedParser);

        return Result.Success();
    }
}