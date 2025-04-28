using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.Domain.Abstractions;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.Domain.Entities.Episodes.SpecialEpisodes;

internal class SpecialEpisodeParser : HtmlNodeParser<SpecialEpisode>
{
    public SpecialEpisodeParser([FromKeyedServices("specialEpisodeValidator")]  IHtmlNodeValidator<SpecialEpisode> validator) : base(validator)
    {
    }

    protected override Result<SpecialEpisode> ParseInternal(HtmlNode input)
    {
        var title = input.SelectSingleNode("./td[1]").InnerText;
        var specialEpisode = new SpecialEpisode(EpisodeId.New(), title);

        return specialEpisode;
    }
}