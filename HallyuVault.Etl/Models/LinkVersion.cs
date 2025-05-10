namespace HallyuVault.Etl.Models
{
    public class LinkVersion
    {
        public int LinkVersionId { get; private set; }
        public Guid GroupId { get; private set; }
        public int ScrapedLinkId { get; private set; }

        public ContainerScrapedLink ScrapedLink { get; private set; }

        private LinkVersion() { }

        public LinkVersion(Guid groupId, ContainerScrapedLink scrapedLink)
        {
            GroupId = groupId;
            ScrapedLink = scrapedLink;
        }
    }
}
