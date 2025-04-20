using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.SpecialEpisodeParsing
{
    public class SpecialEpisodeValidator : ISpecialEpisodeValidator
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
}
