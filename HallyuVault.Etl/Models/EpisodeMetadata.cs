using HallyuVault.Etl.MetadataEnrichment;

namespace HallyuVault.Etl.Models
{
    public class EpisodeMetadata
    {
        public int EpisodeMetadataId { get; set; }
        public Guid? ContainerScrapedLinkVersionId { get; set; }
        public int? RawEpisodeVersionId { get; set; }
        public int? FileNameMetadataId { get; set; }
        public int? MediaMetadataId { get; set; }

        public FileNameMetadata? FileNameMetadata { get; set; }
        public MediaMetadata? MediaMetadata { get; set; }
    }
}