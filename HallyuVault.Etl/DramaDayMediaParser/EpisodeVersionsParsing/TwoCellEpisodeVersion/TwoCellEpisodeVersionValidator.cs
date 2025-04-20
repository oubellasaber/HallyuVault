using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.TwoCellEpisodeVersion
{
    public class TwoCellEpisodeVersionValidator : ITwoCellEpisodeVersionValidator
    {
        public Result Validate(HtmlNode input)
        {
            var nodes = input.SelectNodes(".//td");

            if (nodes.Count != 2)
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
