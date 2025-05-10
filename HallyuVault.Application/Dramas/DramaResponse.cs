namespace HallyuVault.Application.Dramas;

public sealed class DramaResponse
{
    public int DramaId { get; init; }
    public string TmdbId { get; init; }
    public int SeasonNumber { get; init; }
    public string EnglishTitle { get; init; }
    public string OriginalName { get; init; }
    public string Synopsis { get; init; }
    public int ReleaseYear { get; init; }
    public int Status { get; init; }
    public List<string> Genres { get; init; }
    public List<string> Networks { get; init; }
    public string Director { get; init; }
    public string Writer { get; init; }
    public List<string> Cast { get; init; }
    public double Rating { get; init; }
    public string Poster { get; init; }
    public string Backdrop { get; init; }
}