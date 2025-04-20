using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.ThreeCellEpisodeVersion
{
    public class ThreeCellEpisodeVersionValidator : IThreeCellEpisodeVersionValidator
    {
        public Result Validate(HtmlNode input)
        {
            var nodes = input.SelectNodes(".//td");

            if (nodes.Count != 3)
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
