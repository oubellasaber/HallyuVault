namespace HallyuVault.Etl.MetadataEnrichment
{
    public class FileNameMetadata
    {
        public int FileNameMetadataId { get; set; }
        public string FileName { get; set; } = null!;
        public int SeasonNumber { get; set; }
        public int RangeStart { get; set; }
        public int? RangeEnd { get; set; }
        public string? Quality { get; set; }
        public string? ReleaseGroup { get; set; }
        public string? Network { get; set; }
        public string? RipType { get; set; }
        public string? Extension { get; set; }
    }
}
