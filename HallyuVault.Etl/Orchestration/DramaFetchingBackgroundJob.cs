using HallyuVault.Etl.Fetcher;
using HallyuVault.Etl.Infra;
using HallyuVault.Etl.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Quartz;
using System.Threading.Tasks.Dataflow;

namespace HallyuVault.Etl.Orchestration
{
    [DisallowConcurrentExecution]
    public class DramaFetchingBackgroundJob : IJob
    {
        private readonly DatabaseContext _ctx;
        private readonly IDramaDayApiClient _dramaDayApiClient;
        private readonly ITargetBlock<ScrapedDrama> _queue;

        public DramaFetchingBackgroundJob(
            DatabaseContext ctx,
            IDramaDayApiClient dramaDayApiClient,
            ITargetBlock<ScrapedDrama> queue)
        {
            _ctx = ctx;
            _dramaDayApiClient = dramaDayApiClient;
            _queue = queue;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            DateTime? lastFetchedDramaPostUpdateDatetime = await _ctx.ScrapedDramas
                .Select(x => x.UpdatedOnUtc)
                .OrderByDescending(x => x)
                .FirstOrDefaultAsync();

            var posts = await _dramaDayApiClient.GetDramas(lastFetchedDramaPostUpdateDatetime);

            Console.WriteLine("Starting job");

            foreach (var post in posts)
            {
                await _queue.SendAsync(post);
            }

            // update the database with the last fetched post
        }
    }
}