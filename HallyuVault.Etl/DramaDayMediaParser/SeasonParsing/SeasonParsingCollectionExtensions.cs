using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.HorizontalSeason;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.SidebarSeason;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.UrlSeason;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.DramaDayMediaParser.SeasonParsing
{
    public static class SeasonParsingCollectionExtensions
    {
        public static IServiceCollection AddSeasonParsing(this IServiceCollection services)
        {
            services.AddTransient<IHorizontalSeasonValidator, HorizontalSeasonValidator>();
            services.AddTransient<ISidebarSeasonValidator, SidebarSeasonValidator>();
            services.AddTransient<IUrlSeasonValidator, UrlSeasonValidator>();
            services.AddTransient<IHtmlNodeParser<Season>, HorizontalSeasonParser>();
            services.AddTransient<IHtmlNodeParser<Season>, SidebarSeasonParser>();
            services.AddTransient<IHtmlNodeParser<Season>, UrlSeasonParser>();

            return services;
        }
    }
}
