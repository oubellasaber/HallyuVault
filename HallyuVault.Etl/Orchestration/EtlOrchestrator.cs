using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.DramaDayMediaParser;
using HallyuVault.Etl.Infra;
using HallyuVault.Etl.LinkResolving;
using HallyuVault.Etl.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks.Dataflow;

namespace HallyuVault.Etl.Orchestration
{
    public class EtlOrchestrator
    {
        private readonly BufferBlock<ScrapedDrama> _pipelineHead;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _token;
        private readonly DataflowLinkOptions _propagate = new DataflowLinkOptions { PropagateCompletion = true };

        public ITargetBlock<ScrapedDrama> Entry => _pipelineHead;

        public EtlOrchestrator(IServiceProvider sp, CancellationToken token)
        {
            _serviceProvider = sp;
            _token = token;
            _pipelineHead = new BufferBlock<ScrapedDrama>();

            // A scope for the DbContext to track entities throughout the pipeline
            var scope = _serviceProvider.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            // The pipeline blocks
            var htmlToMedia = new TransformBlock<ScrapedDrama, Media>(async post =>
            {
                var parser = scope.ServiceProvider.GetRequiredService<MediaParser>();
                //var result = await parser.Parse(new HtmlNode("ToDo: find a solution here"));
                //if (!result.IsSuccess)
                //{
                //    Console.WriteLine($"Failed to parse drama: {result.Error.Message}");
                //    return null;
                //}
                //return result.Value;
                return null!;
            });

            // Track Media entities in the DbContext
            var trackMedia = new ActionBlock<Media>(async media =>
            {
                if (media == null) return;
                await ctx.AddAsync(media, token);
            });

            var broadcast = new BroadcastBlock<Media>(media => media);

            var explode = new TransformManyBlock<Media, ResolvableEpisode>(media =>
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
                        await ctx.AddAsync(resolvedLink, token); // Update the db ctx
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
                    .AnyAsync(x => x.DramaId == post.DramaId);

                if (!isDramaExists)
                    await htmlToMedia.SendAsync(post);
                //else
                //await partialUpdateBlock.SendAsync(post);
            });

            // Link the blocks
            _pipelineHead.LinkTo(gateway, _propagate);
            htmlToMedia.LinkTo(broadcast, _propagate);
            broadcast.LinkTo(trackMedia, _propagate, media => media != null);
            broadcast.LinkTo(explode, _propagate, media => media != null);
            broadcast.LinkTo(DataflowBlock.NullTarget<Media>(), media => media == null);
            explode.LinkTo(resolveLinks, _propagate);

            // Handle pipeline completion
            resolveLinks.Completion.ContinueWith(async _ =>
            {
                try
                {
                    // If we reach here, the pipeline completed successfully
                    // Save all tracked changes
                    await ctx.SaveChangesAsync(token);
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
            }, token);
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