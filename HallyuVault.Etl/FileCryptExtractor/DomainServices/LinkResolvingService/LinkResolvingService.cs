﻿using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace HallyuVault.Etl.FileCryptExtractor.DomainServices.LinkResolvingService;

public sealed class LinkResolvingService
{
    private readonly HttpClient _client;
    private readonly FileCryptSettings _settings;

    public LinkResolvingService(IHttpClientFactory httpClientFactory,
                                IOptions<FileCryptSettings> settings)
    {
        _client = httpClientFactory.CreateClient("Default");
        _settings = settings.Value;
    }

    public async Task<Result<Uri>> Resolve(string id, FileCryptHeader fileCryptHeader)
    {
        var requestUrl = new Uri(_settings.BaseUrl, $"{_settings.LinkEndpoint}/{id}.html");

        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        request.Headers.Add(fileCryptHeader.PhpSessionCookie.HeaderName, fileCryptHeader.PhpSessionCookie.Value);

        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return Result.Failure<Uri>(
                new Error("LinkResolvingService.HttpRequestFailed", $"Failed to fetch the html.")
            );
        }

        string content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return Result.Failure<Uri>(
                new Error("LinkResolvingService.EmptyResponse", "The fetched HTML content is empty.")
            );
        }

        Regex regex = new Regex(@"href='(?<redirect>[^']*)'");

        Match match = regex.Match(content);

        if (!match.Success)
            return Result.Failure<Uri>(
                new Error("FileCrypt.HtmlHasNoRedirectUrl", "The returned html does not contain the redirect url")
                );

        string resolvedUrl = match.Groups["redirect"].Value;

        if (!Uri.TryCreate(resolvedUrl, UriKind.Absolute, out var resolvedUri))
        {
            return Result.Failure<Uri>(
                new Error("LinkResolvingService.InvalidRedirectUrl", "The resolved URL is not a valid absolute URI.")
                );
        }

        var finalReq = new HttpRequestMessage(HttpMethod.Get, resolvedUrl);

        finalReq.Headers.Add(fileCryptHeader.PhpSessionCookie.HeaderName, fileCryptHeader.PhpSessionCookie.Value);
        try
        {
            response = await _client.SendAsync(finalReq);
        }
        catch (Exception e)
        {

            return response?.RequestMessage?.RequestUri;
        }

        return response?.RequestMessage?.RequestUri;
    }
}