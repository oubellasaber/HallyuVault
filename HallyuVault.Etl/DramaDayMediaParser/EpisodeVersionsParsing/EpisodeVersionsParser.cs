using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing
{
    public class EpisodeVersionsParser : HtmlNodeParser<List<EpisodeVersion>>
    {
        private readonly IEnumerable<IHtmlNodeParser<List<EpisodeVersion>>> _parsers;

        // it takes a quality group validator besides the base html validator
        public EpisodeVersionsParser(
            IEpisodeVersionsValidator validator,
            IEnumerable<ISpecializedEpisodeVersionParser> parsers) :
            base(validator)
        {
            _parsers = parsers;
        }

        protected override Result<List<EpisodeVersion>> ParseInternal(HtmlNode input)
        {
            foreach (var parser in _parsers)
            {
                var parsingResult = parser.Parse(input);
                if (parsingResult.IsSuccess)
                {
                    return parsingResult;
                }
            }

            return Result.Failure<List<EpisodeVersion>>(HtmlParsingErrors.ParsingFailed);
        }
    }
}
