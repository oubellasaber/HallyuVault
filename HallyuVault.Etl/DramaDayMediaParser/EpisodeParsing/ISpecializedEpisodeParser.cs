using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Models;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing
{
    public interface ISpecializedEpisodeParser<T> : IHtmlNodeParser<T> where T : Episode;
}
