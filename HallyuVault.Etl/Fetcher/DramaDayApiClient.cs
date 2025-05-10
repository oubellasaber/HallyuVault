using HallyuVault.Etl.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace HallyuVault.Etl.Fetcher
{
    public class DramaDayApiClient : IDramaDayApiClient
    {
        private readonly string _postEndpoint;
        private readonly HttpClient _httpClient;
        private readonly FetchingOptions _fetchingOptions;

        public DramaDayApiClient(HttpClient httpClient, IOptions<FetchingOptions> fetchingOptions)
        {
            _httpClient = httpClient;
            _fetchingOptions = fetchingOptions.Value;
            _postEndpoint = "https://dramaday.me/wp-json/wp/v2/posts";
        }

        public async Task<IEnumerable<ScrapedDrama>> GetDramas(DateTime? modifiedAfter)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "categories", _fetchingOptions.Categories.ToString() },
                { "orderby", _fetchingOptions.OrderBy },
                { "order", _fetchingOptions.Order },
                { "per_page", _fetchingOptions.PageSize.ToString() },
                { "after", "2025-01-01T00:00:00" }
            };

            if (modifiedAfter != null)
            {
                queryParams.Add("modified_after", modifiedAfter.Value.ToString("yyyy-MM-ddTHH:mm:ss"));
            }
            else
            {
                var startOf2025 = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                string formatted = startOf2025.ToString("yyyy-MM-ddTHH:mm:ss");
                queryParams.Add("modified_after", formatted);
            }

            var uriBuilder = new UriBuilder(_postEndpoint)
            {
                Query = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))
            };

            DateTime pulledOnUtc = DateTime.UtcNow;
            var response = await _httpClient.GetAsync(uriBuilder.Uri);

            response.EnsureSuccessStatusCode();

            var jsonContent = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions();
            options.Converters.Add(new DramaPostConverter());
            var dramas = JsonSerializer.Deserialize<List<ScrapedDrama>>(jsonContent, options) ?? [];

            dramas.ForEach(x => x.PulledOn = pulledOnUtc);

            return dramas;
        }
    }
}
