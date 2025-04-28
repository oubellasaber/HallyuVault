using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.Domain.Entities.Episodes.UnknownEpisodes;

public class UnknownEpisodeParser : HtmlNodeParser<UnknownEpisode>
{
    public UnknownEpisodeParser([FromKeyedServices("unknownEpisodeValidator")] IHtmlNodeValidator<HtmlNode> validator) : base(validator)
    {
    }

    protected override Result<UnknownEpisode> ParseInternal(HtmlNode input)
    {
        var rawTitle = input.SelectSingleNode("./td[1]").InnerText;
        var unknownEpisode = new UnknownEpisode(EpisodeId.New(), rawTitle);

        return unknownEpisode;
    }
}