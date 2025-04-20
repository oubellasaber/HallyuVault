using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing
{
    public class SeasonParser : HtmlNodeParser<Season>
    {
        private readonly IEnumerable<IHtmlNodeParser<Season>> _parsers;

        public SeasonParser(
            IHtmlNodeValidator validator,
            IEnumerable<IHtmlNodeParser<Season>> parsers) :
            base(validator)
        {
            _parsers = parsers;
        }

        protected override Result<Season> ParseInternal(HtmlNode input)
        {
            foreach (var parser in _parsers)
            {
                var parsingResult = parser.Parse(input);
                if (parsingResult.IsSuccess)
                {
                    return parsingResult;
                }
            }

            return Result.Failure<Season>(HtmlParsingErrors.ParsingFailed);
        }
    }
}
