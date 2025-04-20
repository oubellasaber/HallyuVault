using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing.HorizontalMediaVersion
{
    public class HorizontalMediaVersionParser : HtmlNodeParser<MediaVersion>
    {
        public HorizontalMediaVersionParser(IHorizontalMediaVersionValidator validator) : base(validator)
        {
        }

        protected override Result<MediaVersion> ParseInternal(HtmlNode input)
        {
            var tds = input.SelectNodes("./td");
            int tdCount = tds.Count;

            string versionName;

            if (tdCount == 2)
            {
                versionName = input.SelectSingleNode("./td[2]").InnerText;

            }
            else // tdCount == 3
            {
                versionName = input.SelectSingleNode("./td[1]").InnerText;
            }

            return new MediaVersion(versionName);
        }
    }
}
