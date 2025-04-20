using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.BatchEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.SpecialEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.StandardEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.UnknownEpisodeParsing;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing
{
    public static class EpisodeParsingCollectionExtensions
    {
        public static IServiceCollection AddEpisodeParsing(this IServiceCollection services)
        {
            services.AddTransient<IStandardEpisodeValidator, StandardEpisodeValidator>();
            services.AddTransient<IBatchEpisodeValidator, BatchEpisodeValidator>();
            services.AddTransient<ISpecialEpisodeValidator, SpecialEpisodeValidator>();
            services.AddTransient<IUnknownEpisodeValidator, UnknownEpisodeValidator>();

            services.AddTransient<ISpecializedEpisodeParser<StandardEpisode>, StandardEpisodeParser>();
            services.AddTransient<ISpecializedEpisodeParser<SpecialEpisode>, SpecialEpisodeParser>();
            services.AddTransient<ISpecializedEpisodeParser<BatchEpisode>, BatchEpisodeParser>();
            services.AddTransient<ISpecializedEpisodeParser<UnknownEpisode>, UnknownEpisodeParser>();

            services.AddTransient<IHtmlNodeParser<Episode>, EpisodeParser>();

            return services;
        }
    }
}
