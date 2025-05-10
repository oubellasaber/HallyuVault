using HallyuVault.Etl.DownloadLinkExtractors;
using HallyuVault.Etl.DramaDayMediaParser.Abtractions;
using HallyuVault.Etl.FileCryptExtractor;
using HallyuVault.Etl.FileNameExtractors;
using HallyuVault.Etl.Infra;
using HallyuVault.Etl.LinkResolving;
using HallyuVault.Etl.MetadataEnrichment;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks.Dataflow;

namespace HallyuVault.Etl.Orchestration
{
    public class EtlOrchestrator
    {
        private readonly EtlContext _databaseContext;
        private readonly HttpClient _client;
        private readonly IHtmlNodeParser<Media> _htmlNodeParser;
        private readonly ILinkResolver _linkResolver;
        private readonly FileCryptParsingService _fileCryptParsingService;
        private readonly FileNameExtractor _fileNameExtractor;
        private readonly DownloadLinkExtractor _downloadLinkExtractor;
        private readonly MediaMetadataExtractor _mediaMetadataExtractor;
        private readonly DataflowLinkOptions _propagate = new DataflowLinkOptions { PropagateCompletion = true };

        public EtlOrchestrator(
            HttpClient client,
            IHtmlNodeParser<Media> htmlNodeParser,
            ILinkResolver linkResolver,
            EtlContext databaseContext,
            FileCryptParsingService fileCryptParsingService,
            FileNameExtractor fileNameExtractor,
            DownloadLinkExtractor downloadLinkExtractor,
            MediaMetadataExtractor mediaMetadataExtractor)
        {
            _client = client;
            _htmlNodeParser = htmlNodeParser;
            _linkResolver = linkResolver;
            _databaseContext = databaseContext;
            _fileCryptParsingService = fileCryptParsingService;
            _downloadLinkExtractor = downloadLinkExtractor;
            _fileNameExtractor = fileNameExtractor;
            _mediaMetadataExtractor = mediaMetadataExtractor;
        }


        public async Task RunPipeline(ScrapedDrama drama)
        {
            var isDramaExists = await _databaseContext.ScrapedDramas
                    .AnyAsync(x => x.ScrapedDramaId == drama.ScrapedDramaId);

            if (isDramaExists)
            {
                Console.WriteLine("skipped processing drama");

                return;
            }

            Media? media = null;

            try
            {
                Console.WriteLine("Started Parsing the html");
                var response = await _client.GetAsync($"https://dramaday.me/?p={drama.ScrapedDramaId}");
                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var mediaResult = _htmlNodeParser.Parse(doc.DocumentNode);
                if (mediaResult.IsFailure)
                {
                    Console.WriteLine($"Failed to parse drama: {mediaResult.Error}");
                    return;
                }
                _databaseContext.Set<ScrapedDrama>().Add(drama);
                _databaseContext.Set<Media>().Add(mediaResult.Value);

                media = mediaResult.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing post {drama.ScrapedDramaId}: {ex.Message}");
            }

            var explodedMedia = media?.Seasons
                .SelectMany(season =>
                    season.MediaVersions.SelectMany(mediaVersion =>
                        mediaVersion.Episodes.SelectMany(episode =>
                            episode.EpisodeVersions.Select(episodeVersion =>
                                new ResolvableEpisode(season, mediaVersion, episode, episodeVersion)
                            )
                        )
                    ))
                .ToList();

            foreach (var item in explodedMedia!)
            {
                try
                {
                    bool isSuccess = false;
                    foreach (var link in item.Links)
                    {
                        var resolutionResult = await _linkResolver.ResolveAsync(link.RawLink.Url.ToString());
                        if (resolutionResult.IsSuccess)
                        {
                            isSuccess = true;
                            Uri url = new Uri(resolutionResult.Value);
                            var resolvedLink = new ResolvedLink(url, link.RawLink);
                            link.SetResolvedLink(resolvedLink);
                            _databaseContext.Set<ResolvedLink>().Add(resolvedLink);
                        }
                    }

                    if (!isSuccess)
                    {
                        Console.WriteLine("Failed to  process episode");
                        continue;
                    }

                    Console.WriteLine("Successfully processed episode");

                    switch (item.Type)
                    {
                        case EpisodeType.Ranged:
                            var resolvedLinks = item.Links.Where(x => x.ResolvedLink != null);
                            var fcLink = resolvedLinks.FirstOrDefault(x => x.ResolvedLink!.ResolvedLinkUrl.Host.Contains("filecrypt"));

                            if (fcLink != default)
                            {
                                var extractionResult = await _fileCryptParsingService.ParseAsync(fcLink.ResolvedLink!.ResolvedLinkUrl);
                                
                                if (extractionResult.IsSuccess)
                                {
                                    Console.WriteLine($"Extraction done seccufully for filecrypt container {fcLink.ResolvedLink!.ResolvedLinkUrl}");
                                    var linkContainer = new LinkContainer(extractionResult.Value, fcLink.ResolvedLink);

                                    _databaseContext.Set<LinkContainer>().Add(linkContainer);


                                    Console.WriteLine("Grouping links started");
                                    var groupedLinks = LinkGroupingService.GroupLinks(linkContainer.FileCryptLinks.ToList());

                                    foreach (var group in groupedLinks)
                                    {
                                        var guidId = Guid.NewGuid();
                                        foreach (var link in group.Value)
                                        {
                                            _databaseContext.Set<LinkVersion>().Add(new LinkVersion(guidId, link.ContainerScrapedLink));
                                        }

                                        if (group.Value.First().FileName?.IsZipped() ?? false)
                                        {
                                            continue;
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

                                        _databaseContext.Set<EpisodeMetadata>().Add(episodeMetadata);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Extraction failed for filecrypt container {fcLink.ResolvedLink!.ResolvedLinkUrl}, Error: {extractionResult.Error}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"None of the resolved links is supported at the moment ({string.Join(",", resolvedLinks.Select(x => x.ToString()))})");
                            }
                            break;

                        case EpisodeType.Standard:
                        case EpisodeType.Special:
                            Console.WriteLine("Standard, Special not yet supported");
                            // Handle standard episode
                            break;
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            try
            {
                // Save changes after all processing is complete
                await using var transaction = await _databaseContext.Database.BeginTransactionAsync();
                try
                {
                    // Your pipeline operations
                    await _databaseContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine("Pipeline completed successfully and changes saved.");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pipeline failed: {ex.Message}");
                throw;
            }
        }

        /*public async Task RunPipeline(ScrapedDrama drama)
        {
            var httpClient = new HttpClient();

            // The pipeline blocks
            var htmlToMedia = new TransformBlock<(bool, ScrapedDrama), Media?>(async post =>
            {
                try
                {
                    Console.WriteLine("Started Parsing the html");
                    var response = await httpClient.GetAsync($"https://dramaday.me/?p={post.Item2.ScrapedDramaId}");
                    var html = await response.Content.ReadAsStringAsync();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var result = _htmlNodeParser.Parse(doc.DocumentNode);
                    if (result.IsFailure)
                    {
                        Console.WriteLine($"Failed to parse drama: {result.Error}");
                        return null;
                    }
                    _databaseContext.Set<ScrapedDrama>().Add(post.Item2);
                    _databaseContext.Set<Media>().Add(result.Value);

                    return result.Value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing post {post.Item2.ScrapedDramaId}: {ex.Message}");
                    return null;
                }
            });

            var broadcast = new BroadcastBlock<Media?>(media => media);

            var explode = new TransformManyBlock<Media?, ResolvableEpisode>(media =>
            {
                if (media == null) return Enumerable.Empty<ResolvableEpisode>();
                return media.Seasons
                    .SelectMany(season =>
                        season.MediaVersions.SelectMany(mediaVersion =>
                            mediaVersion.Episodes.SelectMany(episode =>
                                episode.EpisodeVersions.Select(episodeVersion =>
                                    new ResolvableEpisode(season, mediaVersion, episode, episodeVersion)
                                )
                            )
                        ));
            });

            var resolveLinks = new TransformBlock<ResolvableEpisode, Result<ResolvableEpisode>>(async resolvableEpisode =>
            {
                try
                {
                    bool isSuccess = false;
                    foreach (var link in resolvableEpisode.Links)
                    {
                        var resolutionResult = await _linkResolver.ResolveAsync(link.RawLink.Url.ToString());
                        if (resolutionResult.IsSuccess)
                        {
                            isSuccess = true;
                            Uri url = new Uri(resolutionResult.Value);
                            var resolvedLink = new ResolvedLink(url, link.RawLink);
                            _databaseContext.Set<ResolvedLink>().Add(resolvedLink);
                        }
                    }

                    return isSuccess
                        ? Result.Success(resolvableEpisode)
                        : Result.Failure<ResolvableEpisode>(new Error("LinkResolutionFailed", "Failed to resolve any links"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error resolving links: {ex.Message}");
                    return Result.Failure<ResolvableEpisode>(new Error("LinkResolutionException", ex.Message));
                }
            });

            var saveResults = new ActionBlock<Result<ResolvableEpisode>>(result =>
            {
                Console.WriteLine(result.IsSuccess
                    ? "Successfully processed episode"
                    : $"Failed to process episode: {result.Error}");
            });

            var inputBlock = new TransformBlock<ScrapedDrama, (bool, ScrapedDrama)>(async post =>
            {
                // Check if drama exists
                var isDramaExists = await _databaseContext.ScrapedDramas
                    .AnyAsync(x => x.ScrapedDramaId == post.ScrapedDramaId);

                // Only pass it through if it doesn't exist
                return (isDramaExists, post);
            });

            // Link the blocks
            inputBlock.LinkTo(htmlToMedia, _propagate, result => !result.Item1);
            htmlToMedia.LinkTo(explode, _propagate, media => media != null);
            //broadcast.LinkTo(explode, _propagate, media => media != null);
            //broadcast.LinkTo(DataflowBlock.NullTarget<Media?>(), _propagate, media => media == null);
            explode.LinkTo(resolveLinks, _propagate);
            resolveLinks.LinkTo(saveResults, _propagate);

            // Send the input
            await inputBlock.SendAsync(drama);

            // Complete the head of the pipeline
            inputBlock.Complete();

            // Wait for the entire pipeline to complete
            try
            {
                await Task.WhenAll(
                    inputBlock.Completion,
                    htmlToMedia.Completion,
                    explode.Completion,
                    resolveLinks.Completion,
                    saveResults.Completion
                );

                // Save changes after all processing is complete
                await using var transaction = await _databaseContext.Database.BeginTransactionAsync();
                try
                {
                    // Your pipeline operations
                    await _databaseContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    Console.WriteLine("Pipeline completed successfully and changes saved.");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Pipeline failed: {ex.Message}");
                throw;
            }
        }*/
    }
}

/* public async Task RunPipelineV1(ScrapedDrama drama)
 {
     await using (var scope = _serviceScopeFactory.CreateAsyncScope())
     {
         var ctx = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

         // The pipeline blocks
         var htmlToMedia = new TransformBlock<ScrapedDrama, Media?>(async post =>
         {
             var scope = _serviceScopeFactory.CreateScope();
             var ctx = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
             var parser = scope.ServiceProvider.GetRequiredService<MediaParser>();
             var httpClient = new HttpClient();
             var response = await httpClient.GetAsync($"https://dramaday.me/?p={post.ScrapedDramaId}");
             var html = await response.Content.ReadAsStringAsync();
             var doc = new HtmlDocument();
             doc.LoadHtml(html);

             var result = parser.Parse(doc.DocumentNode.ParentNode);
             if (result.IsFailure)
             {
                 Console.WriteLine($"Failed to parse drama: {result.Error}");
                 // ToDo ?? failed ? what to do
                 return null;
             }

             await ctx.ScrapedDramas.AddAsync(post);
             await ctx.Media.AddAsync(result.Value);

             return result.Value;
         });

         var broadcast = new BroadcastBlock<Media?>(media => media);

         var explode = new TransformManyBlock<Media?, ResolvableEpisode>(media =>
         {
             if (media == null) return Enumerable.Empty<ResolvableEpisode>();
             return media.Seasons
                 .SelectMany(season =>
                     season.MediaVersions.SelectMany(mediaVersion =>
                         mediaVersion.Episodes.SelectMany(episode =>
                             episode.EpisodeVersions.Select(episodeVersion =>
                                 new ResolvableEpisode(season, mediaVersion, episode, episodeVersion)
                             )
                         )
                     ));
         });

         // genral metadat drama enrichement
         //var enrichMeta = new ActionBlock<Media>(async media =>
         //{
         //    var enricher = sp.GetRequiredService<IMetadataEnricher>();
         //    await enricher.EnrichAsync(media);
         //});

         var resolveLinks = new TransformBlock<ResolvableEpisode, Result<ResolvableEpisode>>(async resolvableEpisode =>
         {
             bool isSuccess = false;
             var resolver = scope.ServiceProvider.GetRequiredService<ILinkResolver>();
             foreach (var link in resolvableEpisode.Links)
             {
                 var resolutionResult = await resolver.ResolveAsync(link.RawLink.Url.ToString());
                 if (resolutionResult.IsSuccess)
                 {
                     isSuccess = true;
                     Uri url = new Uri(resolutionResult.Value);
                     var resolvedLink = new ResolvedLink(url, link.RawLink);
                     await ctx.AddAsync(resolvedLink); // Update the db ctx
                 }
             }

             return isSuccess
                 ? Result.Success(resolvableEpisode)
                 : Result.Failure<ResolvableEpisode>(new Error("LinkResolutionFailed", "Failed to resolve any links"));
         });

         //var broadcastLinks = new BroadcastBlock<ResolvableEpisode>(resolvableEpisode => resolvableEpisode);

         // filter for filecrypt
         // what wpuld happen for ranged without a filecrypt link ??
         //var extractContainerizedEpisodes = new TransformBlock<ResolvableEpisode, FileCryptContainer>(async resolvableEpisode =>
         //{
         //    var filecryptLink = resolvableEpisode.Links.FirstOrDefault(link => link.Url.Host.Contains("filecrypt"));
         //    Debug.Assert(filecryptLink is not null, "the filter should have already verfied that we have at least one filecrypt link");

         //    var extractor = sp.GetRequiredService<FileCryptParsingService>();
         //    var container = await extractor.ParseAsync(filecryptLink.Url);

         //    // Handle it, when resolution fails, or either pass it downstream so it gets saved to the right table
         //    return container.Value;
         //});

         //var saveFileCryptContainer = new ActionBlock<ResolvableEpisode>(async resolvableEpisode =>
         //{

         //});

         // link grouping
         //var groupLinks = new TransformBlock<FileCryptContainer, Dictionary<string, List<Row>>>(container =>
         //{
         //    return new();
         //});

         var gateway = new ActionBlock<ScrapedDrama>(async post =>
         {
             var isDramaExists = await ctx.ScrapedDramas
                 .AnyAsync(x => x.ScrapedDramaId == post.ScrapedDramaId);

             if (!isDramaExists)
                 await htmlToMedia.SendAsync(post);
             //else
             //await partialUpdateBlock.SendAsync(post);
         });

         var pipelineHead = new BroadcastBlock<ScrapedDrama>(default);

         // Link the blocks
         pipelineHead.LinkTo(gateway, _propagate);
         htmlToMedia.LinkTo(broadcast, _propagate);
         broadcast.LinkTo(explode, _propagate, media => media != null);
         explode.LinkTo(resolveLinks, _propagate);


         await pipelineHead.SendAsync(drama);

         pipelineHead.Complete();

         // Handle pipeline completion
         await resolveLinks.Completion.ContinueWith(async _ =>
         {
             try
             {
                 // If we reach here, the pipeline completed successfully
                 // Save all tracked changes
                 await ctx.SaveChangesAsync();
             }
             catch (Exception ex)
             {
                 Console.WriteLine($"Failed to save changes: {ex.Message}");
                 throw;
             }
             finally
             {
                 // Dispose the scope to clean up the DbContext
                 scope.Dispose();
             }
         });
     }
 }

 //private TransformBlock<DramaPost, Media> RegisterFullMediaScrapeWorkflow(IServiceProvider sp, CancellationToken token)
 //{
 //    var mediaParser = sp.GetRequiredService<MediaParser>();
 //    var dbCtx = sp.GetRequiredService<EtlDbContext>();

 //    var mediaParsingBlock = new TransformBlock<DramaPost, Media>(async dramaPost =>
 //    {
 //        // ToDo, use interfaces, IMediaParser .e.x
 //        Media media = mediaParser.Parse(dramaPost);

 //        return media;
 //    });

 //    var uploadMediaBlock = new ActionBlock<Media>(async media =>
 //    {
 //        // Upload media to the database
 //        await dbCtx.Medias.AddAsync(media);
 //        await dbCtx.SaveChangesAsync();
 //    });


 //}
}
}


/*
// ✅ THIS IS YOUR ETL PIPELINE COMPOSED ONCE AND REUSED

public class EtlPipeline
{
private readonly BufferBlock<Post> _head;

public ITargetBlock<Post> Entry => _head;

public EtlPipeline(IServiceProvider sp, CancellationToken token)
{
 _head = new BufferBlock<Post>();

 var gateway = new ActionBlock<Post>(async post =>
 {
     var repo = sp.GetRequiredService<IMediaRepository>();
     if (await repo.IsNewMediaAsync(post))
         await newMediaBlock.SendAsync(post);
     else
         await partialUpdateBlock.SendAsync(post);
 });

 _head.LinkTo(gateway, Propagate);

 // 🔷 NEW MEDIA WORKFLOW
 var htmlToMedia = new TransformBlock<Post, Media>(async post =>
 {
     var parser = sp.GetRequiredService<IHtmlParser>();
     var html = await parser.FetchHtmlAsync(post.Url);
     return parser.Parse(html);
 }, Options(token));

 var saveMedia = new TransformBlock<Media, Media>(async media =>
 {
     var db = sp.GetRequiredService<IMediaSaver>();
     return await db.SaveAndUpdateIdsAsync(media);
 }, Options(token));

 var broadcast = new BroadcastBlock<Media>(media => media);

 var enrichMeta = new ActionBlock<Media>(async media =>
 {
     var enricher = sp.GetRequiredService<IMetadataEnricher>();
     await enricher.EnrichAsync(media);
 }, Options(token));

 var explode = new TransformManyBlock<Media, EpisodeRecord>(media =>
 {
     var flattener = sp.GetRequiredService<IEpisodeFlattener>();
     return flattener.Flatten(media);
 }, Options(token));

 var resolveLinks = new TransformBlock<EpisodeRecord, EpisodeRecord>(async record =>
 {
     var resolver = sp.GetRequiredService<ILinkResolver>();
     return await resolver.ResolveAsync(record);
 }, Options(token));

 var saveLinks = new ActionBlock<EpisodeRecord>(async record =>
 {
     var db = sp.GetRequiredService<IResolvedLinkSaver>();
     await db.SaveAsync(record);
 }, Options(token));

 var renderedOnly = new TransformBlock<EpisodeRecord, EpisodeRecord>(record =>
 {
     return record.Type == EpisodeType.Rendered ? record : null;
 }, Options(token));

 var extractHtml = new ActionBlock<EpisodeRecord>(async record =>
 {
     var extractor = sp.GetRequiredService<IHtmlExtractor>();
     var uploader = sp.GetRequiredService<IUploader>();
     var html = await extractor.ExtractAsync(record);
     await uploader.UploadAsync(html);
 }, Options(token));

 // Wire NEW media pipeline
 htmlToMedia.LinkTo(saveMedia, Propagate);
 saveMedia.LinkTo(broadcast, Propagate);
 broadcast.LinkTo(enrichMeta, Propagate);
 broadcast.LinkTo(explode, Propagate);
 explode.LinkTo(resolveLinks, Propagate);
 resolveLinks.LinkTo(saveLinks, Propagate);
 saveLinks.LinkTo(renderedOnly, Propagate);
 renderedOnly.LinkTo(extractHtml, Propagate);

 newMediaBlock = htmlToMedia; // Head of the new media branch

 // 🔷 PARTIAL UPDATE WORKFLOW (stub for now)
 partialUpdateBlock = new ActionBlock<Post>(post => Task.CompletedTask);
}

private ITargetBlock<Post> newMediaBlock;
private ITargetBlock<Post> partialUpdateBlock;

private static readonly DataflowLinkOptions Propagate = new() { PropagateCompletion = true };

private static ExecutionDataflowBlockOptions Options(CancellationToken token) =>
 new() { MaxDegreeOfParallelism = 4, BoundedCapacity = 100, CancellationToken = token };
}

*/