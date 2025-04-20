using HallyuVault.Etl.ScraperApiClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HallyuVault.Etl.ApiKeyRotator.ScraperApi
{
    public static class ScraperApiKeyRotatorServiceCollectionExtensions
    {
        public static IServiceCollection AddScraperApiKeyRotator(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ScraperApiOptions>(configuration.GetSection("ScraperApiConfig"));
            services.Configure<ApiKeyRotationOptions>(configuration.GetSection("ApiKeyRotationConfig"));

            services.AddSingleton<ScraperApiClient.ScraperApiClient>();
            services.AddSingleton<IScraperApiKeyFactory, ScraperApiKeyFactory>();
            services.AddSingleton<ScraperApiKeyManager>();

            return services;
        }
    }
}
