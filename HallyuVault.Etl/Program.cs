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
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.HorizontalSeason;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.SidebarSeason;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing.UrlSeason;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateApplicationBuilder(args);

// DI
host.Services.AddHttpClient();

// Episode Parsing

host.Services.AddTransient<IStandardEpisodeValidator, StandardEpisodeValidator>();
host.Services.AddTransient<IBatchEpisodeValidator, BatchEpisodeValidator>();
host.Services.AddTransient<ISpecialEpisodeValidator, SpecialEpisodeValidator>();
host.Services.AddTransient<IUnknownEpisodeValidator, UnknownEpisodeValidator>();

host.Services.AddTransient<ISpecializedEpisodeParser<StandardEpisode>, StandardEpisodeParser>();
host.Services.AddTransient<ISpecializedEpisodeParser<SpecialEpisode>, SpecialEpisodeParser>();
host.Services.AddTransient<ISpecializedEpisodeParser<BatchEpisode>, BatchEpisodeParser>();
host.Services.AddTransient<ISpecializedEpisodeParser<UnknownEpisode>, UnknownEpisodeParser>();

host.Services.AddTransient<IHtmlNodeParser<Episode>, EpisodeParser>();

// Episode versions parsing
host.Services.AddTransient<INoTableEpisodeVersionValidator, NoTableEpisodeVersionValidator>();
host.Services.AddTransient<IThreeCellEpisodeVersionValidator, ThreeCellEpisodeVersionValidator>();
host.Services.AddTransient<ITwoCellEpisodeVersionValidator, TwoCellEpisodeVersionValidator>();

host.Services.AddTransient<ISpecializedEpisodeVersionParser, NoTableEpisodeVersionParser>();
host.Services.AddTransient<ISpecializedEpisodeVersionParser, ThreeCellEpisodeVersionParser>();
host.Services.AddTransient<ISpecializedEpisodeVersionParser, TwoCellEpisodeVersionParser>();

host.Services.AddTransient<IEpisodeVersionsValidator, EpisodeVersionsValidator>();
host.Services.AddTransient<IHtmlNodeParser<List<EpisodeVersion>>, EpisodeVersionsParser>();

// Media versions parsing
host.Services.AddTransient<IHorizontalMediaVersionValidator, HorizontalMediaVersionValidator>();
host.Services.AddTransient<ISidebarMediaVersionValidator, SidebarMediaVersionValidator>();
host.Services.AddTransient<IHtmlNodeParser<MediaVersion>, HorizontalMediaVersionParser>();
host.Services.AddTransient<IHtmlNodeParser<MediaVersion>, SidebarMediaVersionParser>();

// Season parsing
host.Services.AddTransient<IHorizontalSeasonValidator, HorizontalSeasonValidator>();
host.Services.AddTransient<ISidebarSeasonValidator, SidebarSeasonValidator>();
host.Services.AddTransient<IUrlSeasonValidator, UrlSeasonValidator>();
host.Services.AddTransient<IHtmlNodeParser<Season>, HorizontalSeasonParser>();
host.Services.AddTransient<IHtmlNodeParser<Season>, SidebarSeasonParser>();
host.Services.AddTransient<IHtmlNodeParser<Season>, UrlSeasonParser>();

// Media parsing
host.Services.AddTransient<IHtmlNodeParser<MediaInformation>, MediaInformationParser>();
host.Services.AddTransient<IHtmlNodeParser<Media>, MediaParser>();

// Logging
host.Logging.ClearProviders()
            .AddConsole();                  // ToDo: Add proper logging 

var serviceProvider = host.Services.BuildServiceProvider();
var mediaParser = serviceProvider.GetRequiredService<IHtmlNodeParser<Media>>();

HttpClient client = new HttpClient();
var response = await client.GetAsync("https://dramaday.me/for-eagle-brothers/");
var html = await response.Content.ReadAsStringAsync();
HtmlDocument htmlDocument = new();
htmlDocument.LoadHtml(html);
mediaParser.Parse(htmlDocument.DocumentNode);


// Build
await host.Build().RunAsync();
