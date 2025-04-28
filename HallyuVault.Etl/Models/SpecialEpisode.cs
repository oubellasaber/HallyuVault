namespace HallyuVault.Etl.Models
{
    public class SpecialEpisode : Episode
    {
        public SpecialEpisode(string title)
        {
            Title = title;
        }

        public string Title { get; private set; }
    }
}
