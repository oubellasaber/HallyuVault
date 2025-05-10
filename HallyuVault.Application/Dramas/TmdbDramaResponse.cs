namespace HallyuVault.Application.Dramas;

public sealed class TmdbDramaResponse
{
    public string ImdbId { get; set; }
    public int SeasonNumber { get; set; }
    public string EnglishTitle { get; set; }
    public string OriginalName { get; set; }
    public string Synopsis { get; set; }
    public int ReleaseYear { get; set; }
    public List<string> Genres { get; set; }
    public string OriginalNetwork { get; set; }
    public string Director { get; set; }
    public string Writer { get; set; }
    public List<string> Cast { get; set; }
    public double Rating { get; set; }
    public string Poster { get; set; }
    public string Backdrop { get; set; }
    public List<string> AvailableOn { get; set; }
}