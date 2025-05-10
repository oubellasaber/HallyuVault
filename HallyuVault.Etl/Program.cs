using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Core;
using HallyuVault.Etl.ApiKeyRotator.ScraperApi;
using HallyuVault.Etl.DownloadLinkExtractors;
using HallyuVault.Etl.DramaDayMediaParser;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.BatchEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.SpecialEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.StandardEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing.UnknownEpisodeParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.NoTableEpisodeVersion;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.ThreeCellEpisodeVersion;
using HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing.TwoCellEpisodeVersion;
using HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing.HorizontalMediaVersion;
using HallyuVault.Etl.DramaDayMediaParser.MediaVersionParsing.SidebarMediaVersion;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.HorizontalSeason;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.SidebarSeason;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.UrlSeason;
using HallyuVault.Etl.Fetcher;
using HallyuVault.Etl.FileCryptExtractor;
using HallyuVault.Etl.FileCryptExtractor.DomainServices;
using HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptHeader;
using HallyuVault.Etl.FileNameExtractors;
using HallyuVault.Etl.Infra;
using HallyuVault.Etl.LinkResolving;
using HallyuVault.Etl.MetadataEnrichment;
using HallyuVault.Etl.Models;
using HallyuVault.Etl.Orchestration;
using HallyuVault.Etl.ScraperApiClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;

var host = Host.CreateApplicationBuilder(args);

host.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// DI
host.Services.AddHttpClient(string.Empty, client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});

host.Services.AddHttpClient("NoAutoRedirectClient", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false,
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
});



// Episode Parsing

host.Services.AddSingleton<IStandardEpisodeValidator, StandardEpisodeValidator>();
host.Services.AddSingleton<IBatchEpisodeValidator, BatchEpisodeValidator>();
host.Services.AddSingleton<ISpecialEpisodeValidator, SpecialEpisodeValidator>();
host.Services.AddSingleton<IUnknownEpisodeValidator, UnknownEpisodeValidator>();

host.Services.AddSingleton<StandardEpisodeParser>();
host.Services.AddSingleton<SpecialEpisodeParser>();
host.Services.AddSingleton<BatchEpisodeParser>();
host.Services.AddSingleton<UnknownEpisodeParser>();

host.Services.AddSingleton<IHtmlNodeParser<Episode>, EpisodeParser>();

// Episode versions parsing
host.Services.AddSingleton<INoTableEpisodeVersionValidator, NoTableEpisodeVersionValidator>();
host.Services.AddSingleton<IThreeCellEpisodeVersionValidator, ThreeCellEpisodeVersionValidator>();
host.Services.AddSingleton<ITwoCellEpisodeVersionValidator, TwoCellEpisodeVersionValidator>();

host.Services.AddSingleton<ISpecializedEpisodeVersionParser, NoTableEpisodeVersionParser>();
host.Services.AddSingleton<ISpecializedEpisodeVersionParser, ThreeCellEpisodeVersionParser>();
host.Services.AddSingleton<ISpecializedEpisodeVersionParser, TwoCellEpisodeVersionParser>();

host.Services.AddSingleton<IEpisodeVersionsValidator, EpisodeVersionsValidator>();
host.Services.AddSingleton<IHtmlNodeParser<List<EpisodeVersion>>, EpisodeVersionsParser>();

// Media versions parsing
host.Services.AddSingleton<IHorizontalMediaVersionValidator, HorizontalMediaVersionValidator>();
host.Services.AddSingleton<ISidebarMediaVersionValidator, SidebarMediaVersionValidator>();
host.Services.AddSingleton<IHtmlNodeParser<MediaVersion>, HorizontalMediaVersionParser>();
host.Services.AddSingleton<IHtmlNodeParser<MediaVersion>, SidebarMediaVersionParser>();

// Season parsing
host.Services.AddSingleton<IHorizontalSeasonValidator, HorizontalSeasonValidator>();
host.Services.AddSingleton<ISidebarSeasonValidator, SidebarSeasonValidator>();
host.Services.AddSingleton<IUrlSeasonValidator, UrlSeasonValidator>();
host.Services.AddSingleton<IHtmlNodeParser<Season>, HorizontalSeasonParser>();
host.Services.AddSingleton<IHtmlNodeParser<Season>, SidebarSeasonParser>();
host.Services.AddSingleton<IHtmlNodeParser<Season>, UrlSeasonParser>();

// Media parsing
host.Services.AddSingleton<IHtmlNodeParser<MediaInformation>, MediaInformationParser>();
host.Services.AddSingleton<IHtmlNodeParser<Media>, MediaParser>();

// DramaDay Api
host.Services.AddSingleton<IDramaDayApiClient, DramaDayApiClient>();
host.Services.Configure<FetchingOptions>(host.Configuration.GetSection("FetchingOptions"));
host.Services.Configure<DramaDayOptions>(host.Configuration.GetSection("DramaDayOptions"));

// Link resolvers
host.Services.AddSingleton<ISpecializedLinkResolver, L4sLinkResolver>();
host.Services.AddSingleton<ISpecializedLinkResolver, DramaDayLinkResolver>();
host.Services.AddSingleton<ILinkResolver, LinkResolver>();

// Orchestrator + Db context
host.Services.AddScoped<EtlContext>();
host.Services.AddScoped<EtlOrchestrator>();

// Scraper Api
host.Services.Configure<ScraperApiOptions>(host.Configuration.GetSection("ScraperApiOptions"));
host.Services.Configure<ApiKeyRotationOptions>(host.Configuration.GetSection("ApiKeyRotationOptions"));

host.Services.AddSingleton<ScraperApiClient>();
host.Services.AddSingleton<IApiKeyFactory<ScraperApiKey>, ScraperApiKeyFactory>();
host.Services.AddSingleton<IApiKeyManager<ScraperApiKey, int>, ScraperApiKeyManager>();
host.Services.AddTransient<RequestBuilder>();

// Filecrypt parsing service
host.Services.Configure<FileCryptOptions>(host.Configuration.GetSection("FileCryptOptions"));
host.Services.AddSingleton<FileCryptHeaderExtractionService>();
host.Services.AddSingleton<LinkResolvingService>();
host.Services.AddSingleton<RowParsingService>();
host.Services.AddSingleton<FileCryptParsingService>();
host.Services.AddSingleton<ScraperApiProxy>();

// Download link extractors
host.Services.AddSingleton<IDownloadLinkExtractor, PixeldrainLinkExtractor>();
host.Services.AddSingleton<IDownloadLinkExtractor, SendCmLinkExtractor>();
host.Services.AddSingleton<SendCmLinkExtractor>();
host.Services.AddSingleton<IDownloadLinkExtractor, DatanodesLinkExtractor>();
host.Services.AddSingleton<DatanodesLinkExtractor>();
host.Services.AddSingleton<IDownloadLinkExtractor, BuzzheavierLinkExtractor>();
host.Services.AddSingleton<IDownloadLinkExtractor, GoFileLinkExtractor>();
host.Services.AddSingleton<GoFileLinkExtractor>();
host.Services.AddSingleton<IDownloadLinkExtractor, AkiraBoxLinkExtractor>();
host.Services.AddSingleton<DownloadLinkExtractor>();
host.Services.AddSingleton<MediaMetadataExtractor>();
host.Services.AddSingleton<IFileNameExtractor, PixeldrainFileNameExtractor>();
host.Services.AddSingleton<IFileNameExtractor, SendCmFileNameExtractor>();
host.Services.AddSingleton<IFileNameExtractor, DatanodesFileNameExtractor>();
host.Services.AddSingleton<IFileNameExtractor, BuzzheavierFileNameExtractor>();
host.Services.AddSingleton<IFileNameExtractor, GoFileNameExtractor>();
host.Services.AddSingleton<IFileNameExtractor, AkiraBoxFileNameExtractor>();
host.Services.AddSingleton<FileNameExtractor>();
host.Services.AddSingleton<CloudflareBypasser>();


// Logging
host.Logging.ClearProviders()
            .AddConsole();                  // ToDo: Add proper logging 

// Register Quartz
host.Services.AddQuartz(q =>
{
    Console.WriteLine("configuring job");
    // Use a unique key for the job
    var jobKey = new JobKey("DramaFetchingJob");

    // Register the job
    q.AddJob<DramaFetchingBackgroundJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("TestTrigger")
        .StartNow()
        .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever()) // Runs every 10 min
    );

    // Configure the trigger
    //q.AddTrigger(opts => opts
    //    .ForJob(jobKey)
    //    .WithIdentity("DramaFetchingTrigger")
    //    .WithCronSchedule(configuration["DramaFetchingBackgroundJobOptions:CronSchedule"]) // Cron schedule from config
    //);
});

// Register Quartz hosted service
host.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

//var serviceProvider = host.Services.BuildServiceProvider();
//var mediaParser = serviceProvider.GetRequiredService<IHtmlNodeParser<Media>>();

//HttpClient client = new HttpClient();
//var response = await client.GetAsync("https://dramaday.me/for-eagle-brothers/");
//var html = await response.Content.ReadAsStringAsync();
//HtmlDocument htmlDocument = new();
//htmlDocument.LoadHtml(html);
//var media = mediaParser.Parse(htmlDocument.DocumentNode);


// Build
await host.Build().RunAsync();
