using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.BatchEpisodeParsing
{
    public class BatchEpisodeParser : HtmlNodeParser<Episode>
    {
        public BatchEpisodeParser(IBatchEpisodeValidator validator) : base(validator)
        {
        }

        protected override Result<Episode> ParseInternal(HtmlNode input)
        {
            Regex rangedEpisodesReg = new Regex(@"(\d{1,2})-(\d{1,2})");
            var rangedEps = rangedEpisodesReg.Matches(input.SelectSingleNode("./td[1]").InnerText);

            var firstMatch = rangedEps[0];
            int leftEp = int.Parse(firstMatch.Groups[2].Value);

            var rangeStart = int.Parse(firstMatch.Groups[1].Value);
            var rangeEnd = leftEp;

            var batchEpisode = new BatchEpisode(rangeStart, rangeEnd);

            return batchEpisode;
        }
    }
}
