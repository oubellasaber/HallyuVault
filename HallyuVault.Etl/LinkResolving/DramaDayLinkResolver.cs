using HallyuVault.Core.Abstractions;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.LinkResolving
{
    // https://claude.ai/chat/eb3afb5f-1fde-4e92-8dc2-02ccea966d5b
    public class DramaDayLinkResolver : ISpecializedLinkResolver
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DramaDayLinkResolver(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<string>> ResolveAsync(string link)
        {
            // Get the html content of the first redirect using the default httpclient
            var httpClient = _httpClientFactory.CreateClient();
            var reponse = await httpClient.GetAsync(link);
            var content = await reponse.Content.ReadAsStringAsync();

            // Extract the token and the action from the prev step generated html
            var tokenActionResult = ExtractTokenAndAction(content);
            if (tokenActionResult.IsFailure)
            {
                return tokenActionResult.ConvertFailure<string>();
            }
            var (token, action) = tokenActionResult.Value;

            // Get the final location using the token and the action
            var finalLocationResult = await GetFinalLocation(token, action);
            if (finalLocationResult.IsFailure)
            {
                return finalLocationResult.ConvertFailure<string>();
            }

            var finalLocation = finalLocationResult.Value;

            return finalLocation;
        }

        static Result<(string token, string action)> ExtractTokenAndAction(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var scriptNode = doc.DocumentNode.SelectSingleNode("//script");

            if (scriptNode is null)
            {
                return Result.Failure<(string, string)>(new Error("DramaDayLinkResolver.TokenActionNotFound", "Could not extract the token and the action from the html"));
            }

            string scriptContent = scriptNode.InnerHtml;

            string tokenPattern = @"""token"":""([^""]+)\""";
            string token = Regex.Match(scriptContent, tokenPattern).Groups[1].Value;

            string actionPattern = @"""soralink_z"":""([^""]+)\""";
            string action = Regex.Match(scriptContent, actionPattern).Groups[1].Value;

            return (token, action);
        }

        async Task<Result<string>> GetFinalLocation(string token, string action)
        {
            string url = "https://riviwi.com/wp-admin/admin-ajax.php"; // use config for this, domain may change in the future

            var httpClient = _httpClientFactory.CreateClient("NoAutoRedirectClient");

            var parameters = new Dictionary<string, string>
            {
                ["token"] = token.Replace("\\/", "/"),
                ["action"] = action
            };

            var content = new FormUrlEncodedContent(parameters);

            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            var location = response.Headers.Location?.ToString();

            if (location is null)
            {
                return Result.Failure<string>(LinkResolvingErrors.NoRedirect);
            }

            return location;
        }
    }
}
