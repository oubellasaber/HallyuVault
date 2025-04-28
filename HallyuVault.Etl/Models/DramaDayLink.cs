namespace HallyuVault.Etl.Models
{
    public class DramaDayLink
    {
        public DramaDayLink(string host, Uri url)
        {
            Host = host;
            Url = url;
        }

        public int Id { get; private set; }
        public string Host { get; private set; }
        public Uri Url { get; private set; }
    }
}
