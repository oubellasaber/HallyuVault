using HallyuVault.Core.Abstractions;
using System.Text;
using System.Web;

namespace HallyuVault.Etl.LinkResolving
{
    public class L4sLinkResolver : ISpecializedLinkResolver
    {
        private readonly HttpClient _client;

        public L4sLinkResolver(IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient("NoAutoRedirectClient");
        }


        public async Task<Result<string>> ResolveAsync(string link)
        {
            if (!link.Contains("l4s."))
            {
                return Result.Failure<string>(LinkResolvingErrors.NotSupported);
            }

            // Get the first redirect
            var response = await _client.GetAsync(link);
            string? location = response.Headers.Location?.ToString();
            if (location == null)
            {
                return Result.Failure<string>(LinkResolvingErrors.NoRedirect);
            }

            // Extract the base 64 link from the redirect
            var base64StringResult = ExtractBase64String(location);
            if (base64StringResult.IsFailure)
            {
                return base64StringResult.ConvertFailure<string>();
            }

            // Decode the base 64 link
            var base64String = base64StringResult.Value;
            string decodedString = DecodeBase64(base64String);

            // Extract the direct link from the decoded string
            var directLinkExtractionResult = ExtractDirectUrl(decodedString);
            if (directLinkExtractionResult.IsFailure)
            {
                return Result.Failure<string>(LinkResolvingErrors.NoRedirect);
            }

            var directLink = directLinkExtractionResult.Value;
            return directLink;
        }

        private static Result<string> ExtractBase64String(string url)
        {
            const string prefix = "/tr/";
            int prefixIndex = url.IndexOf(prefix);

            if (prefixIndex == -1)
            {
                return Result.Failure<string>(
                    new Error(
                        "LinkResolvingError.UnexpectedFormat",
                        $"The URL does not contain the expected prefix '{prefix}', so the base64 string cannot be extracted."
                    )
                );
            }

            var base64String = url.Substring(prefixIndex + prefix.Length);

            return base64String;
        }

        private static string DecodeBase64(string base64String)
        {
            byte[] decodedBytes = Convert.FromBase64String(base64String);
            string decodedString = Encoding.UTF8.GetString(decodedBytes);
            return decodedString;
        }

        public static Result<string> ExtractDirectUrl(string url)
        {
            Uri uri = new Uri(url);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);
            string? encodedUrl = queryParams["url"] ?? queryParams["s"];

            if (encodedUrl is null)
            {
                return Result.Failure<string>(
                    new Error(
                        "LinkResolvingError.MissingParameter",
                        "The URL does not contain the required 'url' or 's' query parameter."
                    )
                );
            }

            return Base64String.TryParse(encodedUrl, out var base64String) ?
                base64String.Value :
                encodedUrl;
        }
    }
}
