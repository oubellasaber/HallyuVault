using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.Domain.Entities.Episodes.StandardEpisodes;

public class StandardEpisodeParser : HtmlNodeParser<StandardEpisode>
{
    public StandardEpisodeParser([FromKeyedServices("standardEpisodeValidator")] IHtmlNodeValidator<StandardEpisode> validator) : base(validator)
    {
    }

    protected override Result<StandardEpisode> ParseInternal(HtmlNode input)
    {
        var episodeNumber = int.Parse(
                input.SelectSingleNode("./td[1]")
                .InnerText
                .Substring(0, Math.Min(2, input.SelectSingleNode("./td[1]").InnerText.Length))
            );

        var standardEpisode = new StandardEpisode(EpisodeId.New(), episodeNumber);

        return standardEpisode;
    }
}