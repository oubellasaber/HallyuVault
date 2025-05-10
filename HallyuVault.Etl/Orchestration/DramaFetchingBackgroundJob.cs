using HallyuVault.Etl.Fetcher;
using HallyuVault.Etl.Infra;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace HallyuVault.Etl.Orchestration
{
    [DisallowConcurrentExecution]
    public class DramaFetchingBackgroundJob : IJob
    {
        private readonly EtlContext _ctx;
        private readonly IDramaDayApiClient _dramaDayApiClient;
        private readonly EtlOrchestrator _orchestrator;

        public DramaFetchingBackgroundJob(
            EtlContext ctx,
            IDramaDayApiClient dramaDayApiClient,
            EtlOrchestrator orchestrator)
        {
            _ctx = ctx;
            _dramaDayApiClient = dramaDayApiClient;
            _orchestrator = orchestrator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            DateTime? lastFetchedDramaPostUpdateDatetime = await _ctx.ScrapedDramas
                .OrderByDescending(x => x.UpdatedOn)
                .Select(x => (DateTime?)x.UpdatedOn)
                .FirstOrDefaultAsync();


            var posts = await _dramaDayApiClient.GetDramas(lastFetchedDramaPostUpdateDatetime);

            Console.WriteLine("Starting job");

            foreach (var post in posts)
            {
                await _orchestrator.RunPipeline(post);
            }
        }
    }
}