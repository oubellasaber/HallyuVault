using HallyuVault.Etl.DramaDayMediaParser;
using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing;

namespace HallyuVault.Etl.Models
{
    public class Media
    {
        private readonly List<Season> _seasons = new();

        public Media(string id, string englishTitle, string? koreanTitle)
        {
            Id = id;
            EnglishTitle = englishTitle;
            KoreanTitle = koreanTitle;
        }

        public Media(MediaInformation mediaInformation)
        {
            Id = mediaInformation.DramaDayId;
            EnglishTitle = mediaInformation.EnglishTitle;
            KoreanTitle = mediaInformation.KoreanTitle;
        }

        public string Id { get; private set; }
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
