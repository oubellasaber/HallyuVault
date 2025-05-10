using HallyuVault.Etl.FileCryptExtractor.Entities.Rows.Enums;

namespace HallyuVault.Etl.Models
{
    public class FileCryptLink
    {
        public int Id { get; set; }
        public int FileCryptContainerId { get; set; }
        public string? FileName { get; set; }
        public double? FileSize { get; set; }
        public DataMeasurement? FileUnit { get; set; }
        public Status Status { get; set; }

        // Navigation
        public LinkContainer FileCryptContainer { get; set; } = null!;
        public ContainerScrapedLink ContainerScrapedLink { get; set; }

        private FileCryptLink() { }

        public FileCryptLink(
            string link,
            string? fileName, 
            double? fileSize, 
            DataMeasurement? fileUnit, 
            Status status)
        {
            FileName = fileName;
            FileSize = fileSize;
            FileUnit = fileUnit;
            Status = status;
            
            ContainerScrapedLink = new ContainerScrapedLink(link, this);
        }
    }
}