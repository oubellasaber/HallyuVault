using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.FileCryptExtractor.DomainServices;
using HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptContainer;
using HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader;
using HallyuVault.Etl.ScraperApiClient;
using HtmlAgilityPack;
using System.Text;

namespace HallyuVault.Etl.FileCryptExtractor;

public class FileCryptParsingService
{
    private readonly HttpClient _httpClient;
    private readonly ScraperApiClient.ScraperApiClient _scraperApiClient;
    private readonly RowParsingService _rowParsingService;
    private readonly FileCryptHeaderExtractionService _headerExtractionService;
    private readonly RequestBuilder _requestBuilder;

    public FileCryptParsingService(
        IHttpClientFactory httpClientFactory,
        ScraperApiClient.ScraperApiClient scraperApiClient,
        RowParsingService rowParsingService,
        FileCryptHeaderExtractionService headerExtractionService,
        RequestBuilder requestBuilder)
    {
        _httpClient = httpClientFactory.CreateClient("Default");
        _scraperApiClient = scraperApiClient;
        _rowParsingService = rowParsingService;
        _headerExtractionService = headerExtractionService;
        _requestBuilder = requestBuilder;
    }

    public async Task<Result<FileCryptContainer>> ParseAsync(Uri url, string? password = null)
    {
        var content = password is not null
            ? new StringContent($"pssw={password}", Encoding.UTF8, "application/x-www-form-urlencoded")
            : null;

        //Try scraping with primary HttpClient
        //var result = await TryScrapeWithHttpClientAsync(url, content);
        //if (result.IsSuccess) return result;

        // Try again with ScraperApiClient if captcha was detected
        for (var i = 0; i < 3; i++)
        {
            Console.WriteLine($"Try number {i + 1} for {url}");
            var result = await TryScrapeWithScraperApiAsync(url, content);
            if (result.IsSuccess) return result;
        }

        return Result.Failure<FileCryptContainer>(Error.CaptchaDetected);
    }

    private async Task<Result<FileCryptContainer>> TryScrapeWithHttpClientAsync(Uri url, StringContent? content)
    {
        HttpResponseMessage response;

        if (content is null)
        {
            response = await _requestBuilder
                .ForUrl(url)
                .WithPremium()
                .GetAsync();
        }
        else
        {
            response = await _requestBuilder
                .ForUrl(url)
                .WithPremium()
                .PostAsync(content);
        }

        return await ProcessResponse(url, response);
    }

    private async Task<Result<FileCryptContainer>> TryScrapeWithScraperApiAsync(Uri url, StringContent? content)
    {
        HttpResponseMessage response;

        if (content is null)
        {
            response = await _requestBuilder
                .ForUrl(url)
                .WithPremium()
                .GetAsync();
        }
        else
        {
            response = await _requestBuilder
                .ForUrl(url)
                .WithPremium()
                .PostAsync(content);
        }


        var parsingResult = await ProcessResponse(url, response);
        return parsingResult;
    }

    private async Task<Result<FileCryptContainer>> ProcessResponse(Uri url, HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        if (ContainsCaptcha(html) || ContainsNotFound(html))
            return Result.Failure<FileCryptContainer>(Error.CaptchaDetected);

        var header = _headerExtractionService.GetFileCryptHeader(response);

        HtmlDocument document = new();
        document.LoadHtml(html);
        FileCryptContainer container = new FileCryptContainer(url, document);

        await container.ParseAsync(_rowParsingService, header);
        return container;
    }

    private bool ContainsCaptcha(string responseBody) =>
        responseBody.Contains("Please confirm that you are no robot", StringComparison.OrdinalIgnoreCase);

    private bool ContainsNotFound(string responseBody) =>
        responseBody.Contains("Unfortunately we could not find what you are searching for, we are sorry!", StringComparison.OrdinalIgnoreCase);
}
