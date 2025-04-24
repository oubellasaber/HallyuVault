namespace HallyuVault.Etl.Fetcher
{
    public interface IDramaPostRepository
    {
        Task AddAsync(DramaPost media);
        Task AddBatchAsync(IEnumerable<DramaPost> media);
        Task<DateTime?> GetLastFetchedDramaPostUpdateDatetime();
    }
}