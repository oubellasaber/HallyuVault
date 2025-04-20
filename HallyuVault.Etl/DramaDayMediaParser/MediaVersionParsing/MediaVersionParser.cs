using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing
{
    public class MediaVersionParser : HtmlNodeParser<MediaVersion>
    {
        private readonly IEnumerable<IHtmlNodeParser<MediaVersion>> _parsers;

        public MediaVersionParser(
            IHtmlNodeValidator validator,
            IEnumerable<IHtmlNodeParser<MediaVersion>> parsers) :
            base(validator)
        {
            _parsers = parsers;
        }

        protected override Result<MediaVersion> ParseInternal(HtmlNode input)
        {
            foreach (var parser in _parsers)
            {
                var parsingResult = parser.Parse(input);
                if (parsingResult.IsSuccess)
                {
                    return parsingResult;
                }
            }

            return Result.Failure<MediaVersion>(HtmlParsingErrors.ParsingFailed);
        }
    }
}
