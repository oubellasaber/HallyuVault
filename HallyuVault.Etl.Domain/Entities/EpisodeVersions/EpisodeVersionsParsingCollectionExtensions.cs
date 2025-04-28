using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.NoTableEpisodeVersion;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.ThreeCellEpisodeVersion;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.TwoCellEpisodeVersion;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing
{
    public static class EpisodeVersionsParsingCollectionExtensions
    {
        public static IServiceCollection AddEpisodeVersionsParsing(this IServiceCollection services)
        {
            services.AddTransient<INoTableEpisodeVersionValidator, NoTableEpisodeVersionValidator>();
            services.AddTransient<IThreeCellEpisodeVersionValidator, ThreeCellEpisodeVersionValidator>();
            services.AddTransient<ITwoCellEpisodeVersionValidator, TwoCellEpisodeVersionValidator>();

            services.AddTransient<ISpecializedEpisodeVersionParser, NoTableEpisodeVersionParser>();
            services.AddTransient<ISpecializedEpisodeVersionParser, ThreeCellEpisodeVersionParser>();
            services.AddTransient<ISpecializedEpisodeVersionParser, TwoCellEpisodeVersionParser>();

            services.AddTransient<IEpisodeVersionsValidator, EpisodeVersionsValidator>();
            services.AddTransient<IHtmlNodeParser<List<EpisodeVersion>>, EpisodeVersionsParser>();

            return services;
        }
    }
}
