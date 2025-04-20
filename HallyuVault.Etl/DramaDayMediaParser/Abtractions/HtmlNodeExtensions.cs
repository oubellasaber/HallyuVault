using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.Abtractions
{
    public static class HtmlNodeExtensions
    {
        public static bool IsTableRow(this HtmlNode node)
        {
            return node?.Name.ToLower() == "tr";
        }

        public static bool IsHeader(this HtmlNode node)
        {
            return node?.SelectNodes(".//th")?.Count > 0;
        }

        public static bool IsEmptyRow(this HtmlNode node)
        {
            return node?.SelectNodes(".//td")?.All(td => string.IsNullOrWhiteSpace(td.InnerText)) ?? false;
        }

        public static bool IsPasswordRow(this HtmlNode node)
        {
            return node?.InnerText.Contains("password", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        public static string GetCellText(this HtmlNode row, int index)
        {
            var cells = row.SelectNodes(".//td");
            if (cells == null || index >= cells.Count)
            {
                return string.Empty;
            }

            return cells[index].InnerText.Trim();
        }
    }
}
