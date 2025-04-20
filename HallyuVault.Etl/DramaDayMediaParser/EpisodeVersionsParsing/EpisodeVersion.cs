using HallyuVault.Etl.DramaDayMediaParser.EpisodeParsing;

namespace HallyuVault.Etl.DramaDayMediaParser.EpisodeVersionsParsing
{
    public class EpisodeVersion
    {
        private readonly List<DramaDayLink> _links = new();

        public EpisodeVersion(string name)
        {
            Name = name;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public IReadOnlyCollection<DramaDayLink> Links => _links.AsReadOnly();

        public void AddLink(DramaDayLink link)
        {
            _links.Add(link);
        }

        public void AddLinkRange(IEnumerable<DramaDayLink> links)
        {
            foreach (var link in links)
            {
                _links.Add(link);
            }
        }
    }
}
