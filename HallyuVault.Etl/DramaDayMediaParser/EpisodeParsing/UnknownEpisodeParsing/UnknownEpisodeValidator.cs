using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.UnknownEpisodeParsing
{
    public class UnknownEpisodeValidator : IUnknownEpisodeValidator
    {
        public Result Validate(HtmlNode input)
        {
            if (!string.IsNullOrEmpty(input.SelectSingleNode("./td[1]").InnerText))
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
