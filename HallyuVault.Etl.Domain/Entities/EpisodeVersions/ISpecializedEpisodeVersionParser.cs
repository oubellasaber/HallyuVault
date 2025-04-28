using HallyuVault.Etl.DramaDayMediaParser.Abtractions;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing
{
    public interface ISpecializedEpisodeVersionParser : IHtmlNodeParser<List<EpisodeVersion>>;
}
