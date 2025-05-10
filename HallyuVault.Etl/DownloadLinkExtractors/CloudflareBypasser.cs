using System.Text;
using System.Text.Json;

namespace HallyuVault.Etl.DownloadLinkExtractors
{
    public class CloudflareBypassResult
    {
        public List<Cookie> Cookies { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();
    }

    public class Cookie
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public double? Expires { get; set; }
        public bool HttpOnly { get; set; }
        public bool Secure { get; set; }
    }

    public class CloudflareBypasser
    {
        private readonly string _unflareUrl;

        public CloudflareBypasser(string unflareApiBaseUrl = "http://localhost:5002")
        {
            _unflareUrl = $"{unflareApiBaseUrl.TrimEnd('/')}/scrape";
        }

        public async Task<CloudflareBypassResult?> BypassAsync(string targetUrl, int timeout = 60000)
        {
            using var client = new HttpClient();

            var requestBody = new
            {
                url = targetUrl,
                timeout,
                method = "GET"
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(_unflareUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ API Error: {response.StatusCode}");
                    Console.WriteLine(responseBody);
                    return null;
                }

                var result = JsonSerializer.Deserialize<CloudflareBypassResult>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception during bypass: {ex.Message}");
                return null;
            }
        }
    }

}