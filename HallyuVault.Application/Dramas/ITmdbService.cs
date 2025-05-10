namespace HallyuVault.Application.Dramas
{
    public interface ITmdbService
    {
        Task<TmdbDramaResponse> GetDramaAsync(string imdbId, int season);
    }
}