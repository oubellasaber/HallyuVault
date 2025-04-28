using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser
{
    public class MediaParser : HtmlNodeParser<Media>
    {
        private readonly IHtmlNodeParser<MediaInformation> _mediaInformationParser;
        private readonly IHtmlNodeParser<Season> _seasonParser;
        private readonly IHtmlNodeParser<MediaVersion> _mediaVersionParser;
        private readonly IHtmlNodeParser<Episode> _episodeParser;

        public MediaParser(
            IHtmlNodeParser<MediaInformation> mediaInformationParser,
            IHtmlNodeParser<Season> seasonParser,
            IHtmlNodeParser<MediaVersion> mediaVersionParser,
            IHtmlNodeParser<Episode> episodeParser
            )
        {
            _mediaInformationParser = mediaInformationParser;
            _seasonParser = seasonParser;
            _mediaVersionParser = mediaVersionParser;
            _episodeParser = episodeParser;
        }

        protected override Result<Media> ParseInternal(HtmlNode input)
        {
            var mediaInfo = _mediaInformationParser.Parse(input);

            if (mediaInfo.IsFailure)
            {
                return mediaInfo.ConvertFailure<Media>();
            }

            var seasons = ParseTable(input);

            if (seasons.IsFailure)
            {
                return seasons.ConvertFailure<Media>();
            }

            var media = new Media(mediaInfo.Value);
            media.AddSeasonRange(seasons.Value);

            return media;
        }

        public Result<IReadOnlyCollection<Season>> ParseTable(HtmlNode node)
        {
            var tableBody = node.SelectSingleNode(".//tbody");

            if (tableBody is null)
            {
                return Result.Failure<IReadOnlyCollection<Season>>(new Error("Parser.TableNotFound", "Table body not found."));
            }

            List<HtmlNode> nodes = new();
            nodes.Add(node.OwnerDocument.DocumentNode.SelectSingleNode(@"//meta[@property = ""og:url""]"));
            nodes.AddRange(node.SelectNodes(".//tr"));

            RemoveUncesseryRowsFromTable(nodes);

            var seasons = new List<Season>();

            foreach (var row in nodes)
            {
                var season = _seasonParser.Parse(row);

                if (season.IsSuccess)
                {
                    seasons.Add(season.Value);
                    continue;
                }

                var mediaVersion = _mediaVersionParser.Parse(row);

                if (mediaVersion.IsSuccess)
                {
                    seasons.Last().AddMediaVersion(mediaVersion.Value);
                    continue;
                }

                if (!seasons.Last().MediaVersions.Any())
                {
                    seasons.Last().AddMediaVersion(MediaVersion.Default);
                }

                var episode = _episodeParser.Parse(row);

                if (episode.IsSuccess)
                {
                    seasons.Last().MediaVersions.Last().AddEpisode(episode.Value);
                    continue;
                }

                // TODO: set up proper logging later
                Console.WriteLine($"Unable to parse: {row.InnerHtml}");
            }

            return seasons;
        }

        private static void RemoveUncesseryRowsFromTable(List<HtmlNode> rows)
        {
            if (rows[1].IsHeader())
            {
                rows.RemoveAt(1);
            }

            rows.RemoveAll(r => r.IsEmptyRow() || r.IsPasswordRow());
        }
    }
}
