using HallyuVault.Domain.Abstraction;

namespace HallyuVault.Domain.Entities.Drama;

public sealed class Drama : Entity
{
    public Drama(int id, long imdbId, int seasonNumber) : base(id)
    {
        ImdbId = imdbId;
        SeasonNumber = seasonNumber;
    }

    public long ImdbId { get; private set; }
    public int SeasonNumber { get; private set; }




    //public string EnglishTitle { get; private set; }
    //public string OriginalName { get; private set; }
    //public string Synopsis { get; private set; }
    //public int ReleaseYear { get; private set; }
    //public List<string> Genres { get; private set; }
    //public string OriginalNetwork { get; private set; }
    //public string Director { get; private set; }
    //public string Writer { get; private set; }
    //public List<string> Cast { get; private set; }
    //public double Rating { get; private set; }
    //public string Poster { get; private set; }
    //public string Backdrop { get; private set; }
    //public List<string> AvailableOn { get; private set; }
}