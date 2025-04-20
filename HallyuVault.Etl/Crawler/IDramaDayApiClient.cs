namespace HallyuVault.Etl.Crawler
{
    public interface IDramaDayApiClient
    {
        Task<IEnumerable<DramaPost>> GetDramas(DateTime? modifiedAfter);
    }
}
