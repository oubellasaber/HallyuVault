using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.NoTableEpisodeVersion
{
    public class NoTableEpisodeVersionValidator : INoTableEpisodeVersionValidator
    {
        public Result Validate(HtmlNode input)
        {
            var nodes = input.SelectNodes(".//td");
            if (!nodes.All(n => n.OriginalName == "p"))
                return Result.Failure(HtmlParsingErrors.MismatchedParser);

            return Result.Success();
        }
    }
}
