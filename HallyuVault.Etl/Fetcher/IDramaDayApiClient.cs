using HallyuVault.Etl.Models;

namespace HallyuVault.Etl.Fetcher
{
    public interface IDramaDayApiClient
    {
        Task<IEnumerable<ScrapedDrama>> GetDramas(DateTime? modifiedAfter);
    }
}
