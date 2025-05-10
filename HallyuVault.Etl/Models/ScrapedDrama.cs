namespace HallyuVault.Etl.Models
{
    public class ScrapedDrama
    {
        public int ScrapedDramaId { get; set; }
        //public int DramaId { get; set; }
        public DateTime AddedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime PulledOn { get; set; }
    }
}
