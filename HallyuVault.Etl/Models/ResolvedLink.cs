namespace HallyuVault.Etl.Models
{
    public class ResolvedLink
    {
        public int Id { get; set; }
        public Uri ResolvedLinkUrl { get; set; }
        public DateTime ResolvedAt { get; set; }
        public DramaDayLink ParentRawLink { get; set; }
        //public int ParentRawLinkId { get; set; }

        public ResolvedLink(Uri resolvedLinkUrl, DramaDayLink parentRawLink)
        {
            ResolvedLinkUrl = resolvedLinkUrl;
            ParentRawLink = parentRawLink;
        }
    }
}