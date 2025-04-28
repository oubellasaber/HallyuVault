using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.BatchEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.SpecialEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.StandardEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.UnknownEpisodeParsing;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing
{
    public class EpisodeParser : HtmlNodeParser<Episode>
    {
        private readonly IEnumerable<IHtmlNodeParser<Episode>> _parsers;
        private readonly IHtmlNodeParser<List<EpisodeVersion>> _episodeVersionsParser;

        public EpisodeParser(
        StandardEpisodeParser standardEpisodeParser,
        SpecialEpisodeParser specialEpisodeParser,
        BatchEpisodeParser batchEpisodeParser,
        UnknownEpisodeParser unknownEpisodeParser,
        IHtmlNodeParser<List<EpisodeVersion>> episodeVersionsParser)
        {
            _parsers = new List<IHtmlNodeParser<Episode>>
            {
                standardEpisodeParser,
                specialEpisodeParser,
                batchEpisodeParser,
                unknownEpisodeParser
            };

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
