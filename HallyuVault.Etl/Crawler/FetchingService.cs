using Microsoft.IdentityModel.Tokens;

namespace HallyuVault.Etl.Crawler
{
    public class FetchingService
    {
        private readonly IDramaPostRepository _dramaPostRepository;
        private readonly IDramaDayApiClient _dramaDayApiClient;

        public FetchingService(IDramaPostRepository dramaPostRepository, IDramaDayApiClient dramaDayApiClient)
        {
            _dramaPostRepository = dramaPostRepository;
            _dramaDayApiClient = dramaDayApiClient;
        }

        public async Task Fetch()
        {
            DateTime? lastFetchedDramaPostUpdateDatetime = await _dramaPostRepository.GetLastFetchedDramaPostUpdateDatetime();

            var dramaPosts = await _dramaDayApiClient.GetDramas(lastFetchedDramaPostUpdateDatetime);

            await _dramaPostRepository.AddBatchAsync(dramaPosts);
        }
    }
}