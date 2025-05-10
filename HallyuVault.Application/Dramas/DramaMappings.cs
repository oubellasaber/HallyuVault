namespace HallyuVault.Application.Dramas;

internal static class DramaMappings
{
    public static DramaResponse ToResponse(this PartialDrama partialDrama, TmdbDramaResponse tmdbDrama)
    {
        return new DramaResponse
        {
            DramaId = partialDrama.DramaId,
            TmdbId = tmdbDrama.ImdbId,
            SeasonNumber = tmdbDrama.SeasonNumber,
            EnglishTitle = tmdbDrama.EnglishTitle,
            OriginalName = tmdbDrama.OriginalName,
            Synopsis = tmdbDrama.Synopsis,
            ReleaseYear = tmdbDrama.ReleaseYear,
            Genres = tmdbDrama.Genres ?? new List<string>(),
            Networks = new List<string> { tmdbDrama.OriginalNetwork ?? string.Empty },
            Director = tmdbDrama.Director,
            Writer = tmdbDrama.Writer,
            Cast = tmdbDrama.Cast ?? new List<string>(),
            Rating = tmdbDrama.Rating,
            Poster = tmdbDrama.Poster,
            Backdrop = tmdbDrama.Backdrop
        };
    }
}
