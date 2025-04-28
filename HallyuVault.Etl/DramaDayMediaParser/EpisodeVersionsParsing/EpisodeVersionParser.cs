using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing
{
    public abstract class EpisodeVersionParser : IHtmlNodeParser<List<EpisodeVersion>>
    {
        protected readonly IHtmlNodeValidator Validator;

        protected EpisodeVersionParser(IHtmlNodeValidator validator)
        {
            Validator = validator;
        }

        public Result<List<EpisodeVersion>> Parse(HtmlNode input)
        {
            var validationResult = Validator.Validate(input);
            if (validationResult.IsFailure)
            {
                return Result.Failure<List<EpisodeVersion>>(validationResult.Error);
            }

            var qualities = ParseQualities(input);
            var links = ParseLinks(input);

            var episodeVersions = links
                .Zip(qualities, (links, versionName) =>
                {
                    var episodeVersion = new EpisodeVersion(versionName);
                    episodeVersion.AddLinkRange(links);

                    return episodeVersion;
                })
                .ToList();

            return episodeVersions;
        }

        protected abstract IEnumerable<IEnumerable<DramaDayLink>> ParseLinks(HtmlNode input);
        protected abstract IEnumerable<string> ParseQualities(HtmlNode input);
    }
}
