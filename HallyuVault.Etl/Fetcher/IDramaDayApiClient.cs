namespace HallyuVault.Etl.Fetcher
{
    public interface IDramaDayApiClient
    {
        Task<IEnumerable<DramaPost>> GetDramas(DateTime? modifiedAfter);
    }
}
