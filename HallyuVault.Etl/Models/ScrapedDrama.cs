namespace HallyuVault.Etl.Models
{
    public class ScrapedDrama
    {
        public int DramaId { get; set; }
        public string RenderedTitle { get; set; }
        public string Slug { get; set; }
        public DateTime AddedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public DateTime PulledOn { get; set; }
    }
}
