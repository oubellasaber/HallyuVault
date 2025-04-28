using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Entities.Episodes.BatchEpisodes.ValueObjects;
using HtmlAgilityPack;

namespace HallyuVault.Etl.Domain.Entities.Episodes.BatchEpisodes;

public class BatchEpisode : Episode
{
    public BatchEpisode(EpisodeId id, EpisodeRange episodeRange) : base(id)
    {
        EpisodeRange = episodeRange;
    }

    public EpisodeRange EpisodeRange { get; private set; }

    public static Result<BatchEpisode> CreateFromHtml(HtmlNode node)
    {
        // Use the specifications for validation
        var validator = new BatchEpisodeNodeValidator();
        var validationResult = validator.Validate(node);
        if (validationResult.IsFailure)
            return Result.Failure<BatchEpisode>(validationResult.Error);

        // Parse using the parser
        var parser = new BatchEpisodeParser();
        var parseResult = parser.Parse(node);
        if (parseResult.IsFailure)
            return Result.Failure<BatchEpisode>(parseResult.Error);

        return Result.Success(new BatchEpisode(EpisodeId.New(), parseResult.Value));
    }
}

using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HallyuVault.Etl.Domain.Entities.Episodes.BatchEpisodes.ValueObjects;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.Domain.Entities.Episodes.BatchEpisodes;

public class BatchEpisodeParser : HtmlNodeParser<BatchEpisode>
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

