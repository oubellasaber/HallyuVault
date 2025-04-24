using HallyuVault.Etl.ApiKeyRotator.ScraperApi;
using HallyuVault.Etl.FileCryptExtractor.DomainServices;
using HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.FileCryptExtractor
{
    public static class FileCryptServiceCollectionExtensions
    {
        public static IServiceCollection AddFileCryptApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<FileCryptHeaderConfig>(configuration.GetSection("FileCryptHeaderConfig"));
            services.Configure<FileCryptSettings>(configuration.GetSection("FileCryptSettings"));

            services.AddHttpClient("Default", client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");
            });

            services.AddSingleton<FileCryptHeaderExtractionService>();
            services.AddScraperApiKeyRotator(configuration);
            services.AddSingleton<LinkResolvingService>();
            services.AddSingleton<RowParsingService>();
            services.AddSingleton<FileCryptParsingService>();

            return services;
        }
    }
}
