using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing
{
    public class EpisodeParser : HtmlNodeParser<Episode>
    {
        private readonly IEnumerable<IHtmlNodeParser<Episode>> _parsers;
        private readonly IHtmlNodeParser<List<EpisodeVersion>> _episodeVersionsParser;

        public EpisodeParser(
            ISpecializedEpisodeParser<StandardEpisode> standardEpisodeParser,
            ISpecializedEpisodeParser<SpecialEpisode> specializedEpisodeParser,
            ISpecializedEpisodeParser<BatchEpisode> batchEpisodeParser,
            ISpecializedEpisodeParser<UnknownEpisode> unknownEpisodeParser,
            IHtmlNodeParser<List<EpisodeVersion>> episodeVersionsParser) 
        {
            _parsers = new List<IHtmlNodeParser<Episode>>() {  };
            _episodeVersionsParser = episodeVersionsParser;
        }

        protected override Result<Episode> ParseInternal(HtmlNode input)
        {
            Episode? parsedEpisode = null;

            foreach (var parser in _parsers)
            {
                var parsingResult = parser.Parse(input);
                if (parsingResult.IsSuccess)
                {
                    parsedEpisode = parsingResult.Value;
                    break;
                }
            }

            if (parsedEpisode is null)
            {
                return Result.Failure<Episode>(HtmlParsingErrors.ParsingFailed);
            }

            var episodeVersions = _episodeVersionsParser.Parse(input);

            if (episodeVersions.IsFailure)
            {
                return Result.Failure<Episode>(HtmlParsingErrors.ParsingFailed);
            }

            parsedEpisode.AddEpisodeVersionRange(episodeVersions.Value);

            return parsedEpisode;
        }
    }
}
