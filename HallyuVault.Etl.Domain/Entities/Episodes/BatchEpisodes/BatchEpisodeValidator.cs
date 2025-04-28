using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.Domain.Entities.Episodes.BatchEpisodes;

public class BatchEpisodeValidator : IHtmlNodeValidator
{
    public Result Validate(HtmlNode input)
    {
        var epCell = input.SelectSingleNode("./td[1]");

        if (!Regex.IsMatch(epCell.InnerText, @"\d{1,2}-\d{1,2}"))
            return Result.Failure(HtmlParsingErrors.MismatchedParser);

        return Result.Success();
    }
}