namespace HallyuVault.Etl.MetadataEnrichment
{
    public class MediaMetadata
    {
        public int MediaMetadataId { get; set; }
        public TimeSpan Duration { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string VideoCodecName { get; set; }
        public string AudioCodecName { get; set; }
        public int AudioChannels { get; set; }
        public long FileSize { get; set; } // in bytes
        public List<SubtitleMetadata> Subtitles { get; set; }
    }

    public class SubtitleMetadata
    {
        public int SubtitleMetadataId { get; set; }
        public string Language { get; set; }
        public string CodecName { get; set; }
        public string? Title { get; set; }

        public int MediaMetadataId { get; set; }
        public MediaMetadata MediaMetadata { get; set; }
    }
}
