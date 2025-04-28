using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing
{
    public interface ISpecializedEpisodeVersionParser : IHtmlNodeParser<List<EpisodeVersion>>;
}
