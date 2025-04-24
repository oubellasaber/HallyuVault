using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HallyuVault.Etl.Fetcher
{
    public class DramaDayApiClient : IDramaDayApiClient
    {
        private readonly string _postEndpoint = "https://drama-day.com/wp-json/wp/v2/posts";
        private readonly HttpClient _httpClient;
        private readonly FetchingOptions _fetchingOptions;

        public DramaDayApiClient(HttpClient httpClient, IOptions<FetchingOptions> fetchingOptions)
        {
            _httpClient = httpClient;
            _fetchingOptions = fetchingOptions.Value;
        }

        public async Task<IEnumerable<DramaPost>> GetDramas(DateTime? modifiedAfter)
        {
            var modifiedAfterExclusive = modifiedAfter?.AddSeconds(1);

            var queryParams = new Dictionary<string, string>
            {
                { "categories", _fetchingOptions.Categories.ToString() },
                { "orderby", _fetchingOptions.OrderBy },
                { "order", _fetchingOptions.Order },
                { "per_page", _fetchingOptions.PageSize.ToString() },
            };

            if (modifiedAfterExclusive != null)
            {
                queryParams.Add("modified_after", modifiedAfterExclusive.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"));
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
            var dramas = JsonSerializer.Deserialize<List<DramaPost>>(jsonContent, options) ?? [];

            dramas.ForEach(x => x.PulledOn = pulledOnUtc);

            return dramas;
        }
    }
}
