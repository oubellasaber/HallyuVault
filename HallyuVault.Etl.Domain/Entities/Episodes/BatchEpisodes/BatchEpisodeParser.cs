using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HallyuVault.Etl.Domain.Entities.Episodes.BatchEpisodes.ValueObjects;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.Domain.Entities.Episodes.BatchEpisodes;

public class BatchEpisodeParser: HtmlNodeParser<BatchEpisode>
{
    public BatchEpisodeParser(IHtmlNodeValidator validator) : base(validator)
    {
    }

    protected override Result<BatchEpisode> ParseInternal(HtmlNode input)
    {
        Regex rangedEpisodesReg = new Regex(@"(\d{1,2})-(\d{1,2})");
        var rangedEps = rangedEpisodesReg.Matches(input.SelectSingleNode("./td[1]").InnerText);

        var firstMatch = rangedEps[0];
        int leftEp = int.Parse(firstMatch.Groups[2].Value);

        var rangeStart = int.Parse(firstMatch.Groups[1].Value);
        var rangeEnd = leftEp;

        EpisodeRange range = new EpisodeRange(rangeStart, rangeEnd);
        var batchEpisode = new BatchEpisode(EpisodeId.New(), range);

        return batchEpisode;
    }
}