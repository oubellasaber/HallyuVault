using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DownloadLinkExtractors;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.Fetcher;
using HallyuVault.Etl.FileCryptExtractor;
using HallyuVault.Etl.FileNameExtractors;
using HallyuVault.Etl.Infra;
using HallyuVault.Etl.LinkResolving;
using HallyuVault.Etl.MetadataEnrichment;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks.Dataflow;

namespace HallyuVault.Etl.Orchestration.Blocks
{
    public class DramaDayHtmlExtractor
    {
        private readonly HttpClient _httpClient;
        private readonly IHtmlNodeParser<Media> _htmlNodeParser;
        private readonly ILinkResolver _linkResolver;
        private readonly FileCryptParsingService _fileCryptParsingService;
        private readonly FileNameExtractor _fileNameExtractor;
        private readonly DownloadLinkExtractor _downloadLinkExtractor;
        private readonly MediaMetadataExtractor _mediaMetadataExtractor;
        private readonly DramaDayOptions _dramaDayOptions;
        private readonly ILogger _logger;

        public DramaDayHtmlExtractor(
            HttpClient httpClient,
            IHtmlNodeParser<Media> htmlNodeParser,
            ILinkResolver linkResolver,
            FileCryptParsingService fileCryptParsingService,
            FileNameExtractor fileNameExtractor,
            DownloadLinkExtractor downloadLinkExtractor,
            MediaMetadataExtractor mediaMetadataExtractor,
            IOptions<DramaDayOptions> dramaDayOptions,
            ILogger logger)
        {
            _httpClient = httpClient;
            _htmlNodeParser = htmlNodeParser;
            _linkResolver = linkResolver;
            _fileCryptParsingService = fileCryptParsingService;
            _fileNameExtractor = fileNameExtractor;
            _downloadLinkExtractor = downloadLinkExtractor;
            _mediaMetadataExtractor = mediaMetadataExtractor;
            _dramaDayOptions = dramaDayOptions.Value;
            _logger = logger;
        }

        //public TransformBlock<ScrapedDrama, Result<Media>> BuildPipeline(IServiceProvider serviceProvider)
        //{
        //    var baseTransformer = CreateScrapedMediaTrasformer();
        //    var mediaResultHandlerBlock = CreateMediaResultHandlerBlock(serviceProvider);
        //    var rangedEpisodesHandlerBlock = CreateResolvedRangedEpisodeWithFileCryptLinkBlock(serviceProvider);
        //    var standaloneEpisodesHandlerBlock = CreateResolvedStandaloneEpisodeBlock(serviceProvider);


        //    var resolvedEpisodeHandlerBlock = new ActionBlock<ResolvableEpisode>(ResolvableEpisode =>
        //    {
        //        if (ResolvableEpisode.Type == EpisodeType.Ranged)
        //            rangedEpisodesHandlerBlock(ResolvableEpisode);
        //        else
        //            standaloneEpisodesHandlerBlock(ResolvableEpisode);
        //    });

        //    var linkResolverBlock = CreateLinkResolverBlock(serviceProvider, resolvedEpisodeHandlerBlock);

        //    baseTransformer.LinkTo(mediaResultHandlerBlock, new DataflowLinkOptions { PropagateCompletion = true });
        //    mediaResultHandlerBlock.LinkTo(linkResolverBlock, new DataflowLinkOptions { PropagateCompletion = true });
        //}

        public TransformBlock<ScrapedDrama, Result<Media>> CreateScrapedMediaTrasformer()
        {
            return new TransformBlock<ScrapedDrama, Result<Media>>(async scrapedDrama =>
            {
                var response = await _httpClient.GetAsync(new Uri($"https://{_dramaDayOptions.Host}/?p={scrapedDrama.ScrapedDramaId}"));
                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var media = _htmlNodeParser.Parse(doc.DocumentNode);

                return media;
            });
        }

        public TransformBlock<Result<Media>, List<ResolvableEpisode>> CreateMediaResultHandlerBlock(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<EtlContext>();

            return new TransformBlock<Result<Media>, List<ResolvableEpisode>>(async result =>
            {
                if (result.IsSuccess)
                {
                    // Handle the successful result
                    var media = result.Value;
                    _logger.LogInformation($"Successfully parsed media: {media.MediaId}");
                    // Save to database or perform other actions
                    await dbContext.Media.AddAsync(media);
                    _logger.LogInformation($"Successfully added media to db context: {media.MediaId}");

                    var explodedMedia = media.Seasons
                        .SelectMany(season =>
                            season.MediaVersions.SelectMany(mediaVersion =>
                                mediaVersion.Episodes.SelectMany(episode =>
                                    episode.EpisodeVersions.Select(episodeVersion =>
                                        new ResolvableEpisode(season, mediaVersion, episode, episodeVersion)
                                    )
                                )
                            ))
                        .ToList();
                    _logger.LogInformation($"Successfully exploded the media: {media.MediaId}");

                    return explodedMedia;
                }
                else
                {
                    // Handle the error
                    _logger.LogError($"Error parsing media: {result.Error}");

                    return [];
                }
            });
        }

        public ActionBlock<List<ResolvableEpisode>> CreateLinkResolverBlock(IServiceProvider serviceProvider, ITargetBlock<ResolvableEpisode> resolvedEpisodeHandlerBlock)
        {
            var etlDbContext = serviceProvider.GetRequiredService<EtlContext>();

            return new ActionBlock<List<ResolvableEpisode>>(async resolvableEpisodes =>
            {
                foreach (var resolvableEpisode in resolvableEpisodes)
                {
                    foreach (var link in resolvableEpisode.Links)
                    {
                        var resolutionResult = await _linkResolver.ResolveAsync(link.RawLink.Url.ToString());

                        bool hasResolvedLink = false;
                        if (resolutionResult.IsSuccess)
                        {
                            Uri url = new Uri(resolutionResult.Value);
                            var resolvedLink = new ResolvedLink(url, link.RawLink);
                            link.SetResolvedLink(resolvedLink);
                            etlDbContext.Set<ResolvedLink>().Add(resolvedLink);
                            hasResolvedLink = true;
                        }

                        if (hasResolvedLink)
                        {
                            await resolvedEpisodeHandlerBlock.SendAsync(resolvableEpisode);
                        }
                    }
                }
            });
        }

        public TransformBlock<ResolvableEpisode, LinkContainer?> CreateResolvedRangedEpisodeWithFileCryptLinkBlock(IServiceProvider serviceProvider)
        {
            var etlDbContext = serviceProvider.GetRequiredService<EtlContext>();

            return new TransformBlock<ResolvableEpisode, LinkContainer?>(async resolvedEpisode =>
            {
                var filecryptLink = resolvedEpisode.Links.FirstOrDefault(pair => pair.ResolvedLink?.ResolvedLinkUrl.Host.Contains("filecrypt") ?? false);

                if (filecryptLink != null &&
                    filecryptLink.ResolvedLink != null)
                {
                    var container = await _fileCryptParsingService.ParseAsync(filecryptLink.ResolvedLink.ResolvedLinkUrl);

                    if (container.IsSuccess)
                    {
                        Console.WriteLine($"Extraction done seccufully for filecrypt container {filecryptLink.ResolvedLink.ResolvedLinkUrl}");
                        var linkContainer = new LinkContainer(container.Value, filecryptLink.ResolvedLink);

                        etlDbContext.Set<LinkContainer>().Add(linkContainer);

                        return linkContainer;
                    }
                    else
                    {
                        Console.WriteLine($"Extraction failed for filecrypt container {filecryptLink.ResolvedLink.ResolvedLinkUrl}, Error: {container.Error}"); // How to recover, try later
                        // Strateagy to resume execution after a failed resolution
                        // First: it gonna be one of the two things either the container deos not exist or somethign wrong with scraper api
                    }
                }
                else
                {
                    Console.WriteLine($"None of the resolved links is supported at the moment ({string.Join(",", resolvedEpisode.Links.Select(x => x.ToString()))})");
                }

                return null;
            });
        }

        public TransformBlock<LinkContainer, Dictionary<string, List<FileCryptLink>>> CreateMetadataEnrichementBlock(IServiceProvider serviceProvider)
        {
            var etlDbContext = serviceProvider.GetRequiredService<EtlContext>();

            return new TransformBlock<LinkContainer, Dictionary<string, List<FileCryptLink>>>(async linkContainer =>
            {
                var fileCryptLinks = linkContainer.FileCryptLinks.ToList();

                var groupedLinks = LinkGroupingService.GroupLinks(linkContainer.FileCryptLinks.ToList());

                foreach (var group in groupedLinks)
                {
                    var guidId = Guid.NewGuid();
                    foreach (var link in group.Value)
                    {
                        etlDbContext.Set<LinkVersion>().Add(new LinkVersion(guidId, link.ContainerScrapedLink));
                    }

                    try
                    {
                        var epMetadata = await MetadataEnrichement(group, guidId);

                        if (epMetadata != null)
                        {
                            etlDbContext.Set<EpisodeMetadata>().Add(epMetadata);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine($"Failed to enriche the group with id {guidId} with metadata");
                        // ToDo: How to handle this
                    }
                }

                return groupedLinks;
            });
        }

        public async Task<EpisodeMetadata?> MetadataEnrichement(KeyValuePair<string, List<FileCryptLink>> group, Guid guidId)
        {
            if (group.Value.First().FileName?.IsZipped() ?? false)
            {
                return null;
            }

            string? fileName = null;

            foreach (var filecryptLink in group.Value)
            {
                try
                {
                    // Extract the file name from the filecrypt link
                    fileName = filecryptLink.FileName ??
                        await _fileNameExtractor.ExtractFileNameAsync(new Uri(filecryptLink.ContainerScrapedLink.ScrapedLink));
                    break;
                }
                catch
                {
                    Console.WriteLine($"Failed to Extract download link from {filecryptLink.ContainerScrapedLink.ScrapedLink}");
                    continue;
                }
            }

            FileNameMetadata? fileNameMeatadata = null;

            if (fileName != null)
                fileNameMeatadata = FilenameMetadataExtractor.Extract(fileName);

            var fileCryptLinksOrderedByHost = group.Value.OrderBy(x =>
            {
                switch (new Uri(x.ContainerScrapedLink.ScrapedLink).Host)
                {
                    case "pixeldrain.com":
                        return 1;

                    case "send.cm":
                        return 2;

                    case "datanodes.to":
                        return 3;

                    case "gofile.io":
                        return 4;

                    case "buzzheavier.com":
                        return 5;

                    case "akirabox.com":
                        return 6;

                    case "mega.nz":
                        return 100;

                    default:
                        return int.MaxValue;
                }
            }).ToList();

            Uri? downloadLink = null;
            MediaMetadata? mediaMetadata = null;

            foreach (var fileCryptLink in fileCryptLinksOrderedByHost)
            {
                var scrapedLink = fileCryptLink.ContainerScrapedLink.ScrapedLink;
                try
                {
                    downloadLink = await _downloadLinkExtractor.ExtractDownloadLink(new Uri(scrapedLink));
                }
                catch
                {
                    Console.WriteLine($"Failed to Extract download link from {scrapedLink}");
                    continue;
                }

                try
                {
                    mediaMetadata = await _mediaMetadataExtractor.GetVideoMetadata(downloadLink);
                    break;
                }
                catch
                {
                    Console.WriteLine($"Failed to Extract media metadata from download link {downloadLink}");
                }
            }

            var episodeMetadata = new EpisodeMetadata
            {
                ContainerScrapedLinkVersionId = guidId,
                FileNameMetadata = fileNameMeatadata,
                MediaMetadata = mediaMetadata
            };

            return episodeMetadata;
        }

        // Metadata grabbing
        public Action<ResolvableEpisode> CreateResolvedStandaloneEpisodeBlock(IServiceProvider serviceProvider)
        {
            return new Action<ResolvableEpisode>(async resolvedEpisode =>
            {
                var resolvedLinks = resolvedEpisode
                    .Links
                    .Where(pair => pair.IsResolved)
                    .Select(pair => pair.ResolvedLink)
                    .OrderBy(link =>
                    {
                        switch (link.ResolvedLinkUrl.Host)
                        {
                            case "pixeldrain.com":
                                return 1;

                            case "send.cm":
                                return 2;

                            case "datanodes.to":
                                return 3;

                            case "gofile.io":
                                return 4;

                            case "buzzheavier.com":
                                return 5;

                            case "akirabox.com":
                                return 6;

                            case "mega.nz":
                                return 100;

                            default:
                                return int.MaxValue;
                        }
                    });


            });
        }
    }
}