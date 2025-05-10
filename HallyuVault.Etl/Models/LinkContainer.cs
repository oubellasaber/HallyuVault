using HallyuVault.Etl.FileCryptExtractor.Entities.FileCryptContainer;

namespace HallyuVault.Etl.Models
{
    public class LinkContainer
    {
        public int Id { get; private set; }
        public int ResolvedLinkId { get; private set; }
        public string Title { get; private set; }

        // Navigation
        public ResolvedLink ResolvedLink { get; private set; }
        public ICollection<FileCryptLink> FileCryptLinks { get; private set; } = new List<FileCryptLink>();

        private LinkContainer() { }

        public LinkContainer(FileCryptContainer fileCryptContainer, ResolvedLink resolvedLink)
        {
            ResolvedLink = resolvedLink;
            Title = fileCryptContainer.Title;

            foreach (var row in fileCryptContainer.Rows)
            {
                var fileCryptLink = new FileCryptLink(
                    row.Link.Url.ToString(),
                    row.FileName,
                    row.FileSize?.Size, 
                    row.FileSize?.Unit, 
                    row.Link.Status);

                FileCryptLinks.Add(fileCryptLink);
            }
        }
    }
}
