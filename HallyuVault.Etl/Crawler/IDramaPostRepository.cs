namespace HallyuVault.Etl.Crawler
{
    public interface IDramaPostRepository
    {
        Task AddAsync(DramaPost media);
        Task AddBatchAsync(IEnumerable<DramaPost> media);
        Task<DateTime?> GetLastFetchedDramaPostUpdateDatetime();
    }
}