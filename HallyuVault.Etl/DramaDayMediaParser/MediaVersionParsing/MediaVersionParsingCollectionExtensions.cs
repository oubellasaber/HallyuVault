using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing.HorizontalMediaVersion;
using HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing.SidebarMediaVersion;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing
{
    public static class MediaVersionParsingCollectionExtensions
    {
        public static IServiceCollection AddMediaVersionParsing(this IServiceCollection services)
        {
            services.AddTransient<IHorizontalMediaVersionValidator, HorizontalMediaVersionValidator>();
            services.AddTransient<ISidebarMediaVersionValidator, SidebarMediaVersionValidator>();
            services.AddTransient<IHtmlNodeParser<MediaVersion>, HorizontalMediaVersionParser>();
            services.AddTransient<IHtmlNodeParser<MediaVersion>, SidebarMediaVersionParser>();

            return services;
        }
    }
}
