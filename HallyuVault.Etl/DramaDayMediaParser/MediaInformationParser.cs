using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HtmlAgilityPack;
using System.Web;

namespace HallyuVault.Etl.DramaDayMediaParser
{
    public record MediaInformation(
        string DramaDayId,
        string EnglishTitle,
        string? KoreanTitle
    );

    public class MediaInformationParser : IHtmlNodeParser<MediaInformation>
    {
        public Result<MediaInformation> Parse(HtmlNode input)
        {
            var dramaDayId = ParseMediaId(input);
            var englishTitle = ParseEnglishTitle(input);
            var koreanTitle = ParseKoreanTitle(input);

            var mediaInfo = new MediaInformation(dramaDayId, englishTitle, koreanTitle);

            return mediaInfo;
        }

        public string ParseMediaId(HtmlNode node)
        {
            var url = node
                .SelectSingleNode(@"//link[@rel = ""shortlink""]")
                .GetAttributeValue("href");

            var uri = new Uri(url);

            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            string postId = queryParams["p"]!;

            return postId;
        }

        public string ParseEnglishTitle(HtmlNode node)
        {
            var url = node
                .SelectSingleNode(@"//meta[@property = ""og:url""]")
                .GetAttributeValue("content");

            var title = GetDramaTitleFromUrl(url);

            return title;
        }

        private static string GetDramaTitleFromUrl(string url)
        {
            var uri = new Uri(url);

            string title = uri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)
                .Trim('/')
                .Replace('-', ' ');

            string titleWithoutSeasonData = RemoveSeasonData(title);

            return titleWithoutSeasonData;
        }

        private static string RemoveSeasonData(string title)
        {
            var titleParts = title.Split(' ').ToList();

            if (titleParts.Count > 2 &&
               titleParts[titleParts.Count - 2].Contains("season", StringComparison.OrdinalIgnoreCase))
            {
                titleParts.RemoveAt(titleParts.Count - 2);
            }

            if (titleParts.Count > 1 &&
                int.TryParse(titleParts[titleParts.Count - 1], out int season) &&
                season >= 1 && season < 7)
            {
                titleParts.RemoveAt(titleParts.Count - 1);
            }

            return string.Join(' ', titleParts);
        }

        public string? ParseKoreanTitle(HtmlNode node)
        {
            var titleNode = node.SelectSingleNode("//div[@class='wpb_wrapper']/p[contains(text(), 'Filename')]");
            if (titleNode == null || string.IsNullOrWhiteSpace(titleNode.InnerText))
                return null;

            var parts = titleNode.InnerText.Split(':');
            if (parts.Length < 2)
                return null;

            string titlesJoined = string.Join(":", parts.Skip(1));
            var titlesSeparated = titlesJoined.Split("/", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (titlesSeparated.Length == 0)
                return null;

            var koreanTitle = GetKoreanTitle(titlesSeparated);
            if (string.IsNullOrWhiteSpace(koreanTitle))
                return null;

            return koreanTitle.Split("시즌")[0].Trim();
        }


        private static Predicate<char> IsKoreanChar = (c) => c >= '\u3131' && c <= '\u318E' || c >= '\uAC00' && c <= '\uD7A3';

        private static string? GetKoreanTitle(IEnumerable<string> titles) =>
            titles.FirstOrDefault(t => t.Any(c => IsKoreanChar(c)));
    }
}
