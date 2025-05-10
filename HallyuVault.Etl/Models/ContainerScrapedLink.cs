namespace HallyuVault.Etl.Models
{
    public class ContainerScrapedLink
    {
        public int Id { get; set; }

        public int? ParentResolvedLinkId { get; set; }

        public string ScrapedLink { get; set; } = null!;
        public int? FileCryptLinkId { get; set; }

        // Navigation
        public ContainerScrapedLink? ParentResolvedLink { get; set; }
        public FileCryptLink? FileCryptLink { get; set; }

        private ContainerScrapedLink() { }

        // FileCrpyt Should never have ParentResolvedLink setted
        public ContainerScrapedLink(
            string scrapedLink,
            FileCryptLink fileCryptLink)
        {
            ScrapedLink = scrapedLink;
            FileCryptLink = fileCryptLink;
        }
    }
}