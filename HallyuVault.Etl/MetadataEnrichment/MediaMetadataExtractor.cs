using FFMpegCore;

namespace HallyuVault.Etl.MetadataEnrichment
{
    public class MediaMetadataExtractor
    {
        private readonly HttpClient _client;

        public MediaMetadataExtractor(HttpClient client)
        {
            _client = client;
        }

        public async Task<MediaMetadata> GetVideoMetadata(Uri sourceStream, Dictionary<string, string>? headers = null)
        {
            var mediaInfo = await FFProbe.AnalyseAsync(sourceStream);

            var videoMetadata = new MediaMetadata
            {
                Duration = mediaInfo.Duration,
                Height = mediaInfo.PrimaryVideoStream?.Height ?? 0,
                Width = mediaInfo.PrimaryVideoStream?.Width ?? 0,
                VideoCodecName = mediaInfo.PrimaryVideoStream?.CodecName ?? string.Empty,
                AudioCodecName = mediaInfo.PrimaryAudioStream?.CodecName ?? string.Empty,
                AudioChannels = mediaInfo.PrimaryAudioStream?.Channels ?? 0,
                FileSize = await GetFileSize(sourceStream) ?? 0, // in bytes
                Subtitles = mediaInfo.SubtitleStreams.Select(subtitle => new SubtitleMetadata
                {
                    Language = subtitle.Language!,
                    CodecName = subtitle.CodecName,
                    Title = subtitle.Tags?.TryGetValue("Title", out string? title) ?? false ? title : null
                }).ToList()

            };

            return videoMetadata;
        }

        public async Task<long?> GetFileSize(Uri sourceStream, Dictionary<string, string>? headers = null)
        {
            // Send a HEAD request
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, sourceStream);

            if (headers != null)
            {
                foreach (var (key, value) in request.Headers)
                {
                    request.Headers.Add(key, value);
                }
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            // Get the Content-Length header
            if (response.Content.Headers.ContentLength.HasValue)
            {
                long fileSize = response.Content.Headers.ContentLength.Value;
                return fileSize;
            }

            return null; // Content-Length header not found
        }
    }
}