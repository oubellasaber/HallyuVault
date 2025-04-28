using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing.SidebarMediaVersion
{
    public class SidebarMediaVersionParser : HtmlNodeParser<MediaVersion>
    {
        public SidebarMediaVersionParser(ISidebarMediaVersionValidator validator) : base(validator)
        {
        }

        protected override Result<MediaVersion> ParseInternal(HtmlNode input)
        {
            string mediaVersionName = Regex.Match(input.SelectSingleNode("./td[1]").InnerText,
                                                  @"^\d{1,2}-\d{1,2}\s+(.+)$",
                                                  RegexOptions.Singleline).Groups[1].Value;

            return new MediaVersion(mediaVersionName);
        }
    }
}
