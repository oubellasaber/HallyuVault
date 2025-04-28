using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing
{
    public class SeasonParser : HtmlNodeParser<Season>
    {
        private readonly IEnumerable<IHtmlNodeParser<Season>> _parsers;

        public SeasonParser(IEnumerable<IHtmlNodeParser<Season>> parsers)
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
