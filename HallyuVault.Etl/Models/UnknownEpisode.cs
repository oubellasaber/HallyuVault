namespace HallyuVault.Etl.Models
{
    public class UnknownEpisode : Episode
    {
        public UnknownEpisode(string rawTitle) : base()
        {
            RawTitle = rawTitle;
        }

        public string RawTitle { get; private set; }
    }
}
