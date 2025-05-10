namespace HallyuVault.Domain.Entities
{
    // SoC: filename metadata, media metadata, download links, subtitles (value object)
    public sealed class DownloadOption
    {
        // FileName metadata
        public string FileName { get; set; }
        public int SeasonNumber { get; set; }
        public int RangeStart { get; set; }
        public int? RangeEnd { get; set; }
        public int Quality { get; set; }
        public string? ReleaseGroup { get; set; }
        public string? Network { get; set; }
        public string? RipType { get; set; }

        // Media metadata
        public TimeSpan Duration { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string VideoCodecName { get; set; }
        public string AudioCodecName { get; set; }
        public int AudioChannels { get; set; }
        public long FileSize { get; set; } // in bytes
        public List<Subtitle> Subtitles { get; set; }

        string AudioChannelsCommonName => AudioChannels switch
        {
            1 => "Mono",
            2 => "Stereo",
            3 => "2.1",
            4 => "Quadraphonic",
            5 => "4.1 or 5.0",
            6 => "5.1",
            7 => "6.1",
            8 => "7.1",
            >= 9 and <= 10 => "7.1.2 or Atmos",
            > 10 => "Custom Setup",
            _ => "Unknown or Unsupported"
        };

        public List<Uri> DownloadLinks { get; set; }
    }

    public sealed class Subtitle
    {
        public string Language { get; set; }
        public string CodecName { get; set; }
        public string? Title { get; set; }
    }
}
