using HallyuVault.Etl.DramaDayMediaParser;

namespace HallyuVault.Etl.Models
{
    public class Media
    {
        private readonly List<Season> _seasons = new();

        private Media() { }

        public Media(string id, string englishTitle, string? koreanTitle)
        {
            MediaId = id;
            EnglishTitle = englishTitle;
            KoreanTitle = koreanTitle;
        }

        public Media(MediaInformation mediaInformation)
        {
            MediaId = mediaInformation.DramaDayId;
            EnglishTitle = mediaInformation.EnglishTitle;
            KoreanTitle = mediaInformation.KoreanTitle;
        }

        public string MediaId { get; private set; }
        public string EnglishTitle { get; set; }
        public string? KoreanTitle { get; set; }
        public IReadOnlyCollection<Season> Seasons => _seasons.AsReadOnly();

        public void AddSeason(Season season)
        {
            _seasons.Add(season);
        }

        public void AddSeasonRange(IEnumerable<Season> seasons)
        {
            foreach (var season in seasons)
            {
                _seasons.Add(season);
            }
        }
    }
}
