using HallyuVault.Etl.DramaDayMediaParser.Abtractions;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing
{
    public interface ISpecializedEpisodeParser<T> : IHtmlNodeParser<T> where T : Episode;
}
