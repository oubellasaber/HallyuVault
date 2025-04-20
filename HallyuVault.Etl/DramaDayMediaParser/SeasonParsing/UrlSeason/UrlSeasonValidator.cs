using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.UrlSeason
{
    internal class UrlSeasonValidator : IUrlSeasonValidator
    {
        public Result Validate(HtmlNode input)
        {
            var hasUrlMetaNode = input.GetAttributeValue("property", string.Empty) == "og:url";

            if (!hasUrlMetaNode)
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
