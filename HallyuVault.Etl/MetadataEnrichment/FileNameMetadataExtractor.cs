using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HallyuVault.Etl.MetadataEnrichment
{
    public class FilenameMetadataExtractor
    {
        public static FileNameMetadata Extract(string fileName)
        {
            var (season, episode) = ExtractSeasonAndEpisode(fileName);

            return new FileNameMetadata
            {
                FileName = fileName,
                SeasonNumber = season ?? -1,
                RangeStart = episode ?? -1,
                Quality = GetQuality(fileName),
                ReleaseGroup = GetReleaseGroup(fileName),
                Network = GetNetwork(fileName),
                RipType = GetRipType(fileName),
                Extension = GetExtention(fileName),
            };
        }

        public static string? GetExtention(string filename)
        {
            var segments = filename.Split('.');
            
            if (segments.Length > 1)
            {
                // Get the last segment as the extension
                var lastSegment = segments.Last();
                return lastSegment;
            }

            return null; // No extension found
        }

        public static string? GetQuality(string filename)
        {
            string[] qualities = { "720p", "1080p", "540p", "4K", "480p", "2160p", "1440p" };
            foreach (var quality in qualities)
            {
                if (filename.Contains(quality, StringComparison.OrdinalIgnoreCase))
                {
                    return quality;
                }
            }
            return null;
        }

        public static string? GetReleaseGroup(string filename)
        {
            var pattern = @"-(?<releasegroup>[a-zA-Z0-9]+)[.\s[]*\[?";
            var matches = Regex.Matches(filename, pattern);

            // Get the last match if it exists
            if (matches.Count > 0)
            {
                var lastMatch = matches[matches.Count - 1];
                return lastMatch.Groups["releasegroup"].Value;
            }

            return null;
        }

        public static string? GetNetwork(string filename)
        {
            // Dictionary with abbreviations or alternate names as keys, full names as values
            var networkMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "friday", "friDay" },
            { "viki", "Viki" },
            { "viu", "Viu" },
            { "iq", "iQIYI" },
            { "nf", "Netflix" },
            { "dsnp", "Disney+" },
            { "tving", "TVING" },
            { "hs", "Hulu" },
            { "kcw", "KCW" },
            { "hulu", "Hulu" },
            { "amzn", "Amazon Prime Video" },
            { "wavve", "Wavve" },
            { "sbs", "SBS" },
            { "iqiyi", "iQIYI" },
            { "tvn", "tvN" },
            { "kbs", "KBS" },
            { "jtbc", "JTBC" },
            { "LINETV", "LINE TV"},
            { "kocawa", "Kocowa" },
            { "naver", "Naver" },
            { "cpng", "Coupang Play" },
            { "coupang+play", "Coupang Play" },
            { "watcha", "Watcha" },
            { "wetv", "WeTV" },
            { "pmtp", "Paramount+" },
            { "hbo", "HBO" },
            { "paramount", "Paramount+" }
        };

            foreach (var entry in networkMappings)
            {
                if (filename.Contains(entry.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value; // Return the mapped full name
                }
            }

            return null;
        }

        public static string? GetRipType(string filename)
        {
            // Check if release group implies HDTV
            if (Array.Exists(new[] { "next", "f1rst", "wanna" },
                group => string.Equals(GetReleaseGroup(filename), group, StringComparison.OrdinalIgnoreCase)))
            {
                return "HDTV";
            }

            // Normalize filename once
            var lowerFilename = filename.ToLowerInvariant();

            // Map common rip type terms to canonical forms
            var ripTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "webrip", "WEBRip" },
                { "web-dl", "WEB-DL" },
                { "webdl", "WEB-DL" },
                { "bluray", "BluRay" },
                { "hdrip", "HDRip" },
                { "dvdrip", "DVDRip" },
                { "hdtv", "HDTV" },
                { "hd-tv", "HDTV" }
            };

            foreach (var (key, value) in ripTypeMappings)
            {
                if (lowerFilename.Contains(key))
                {
                    return value;
                }
            }

            return null;
        }


        public static (int? season, int? episode) ExtractSeasonAndEpisode(string filename)
        {
            string pattern = @"[Ss](?:eason)?\s*(\d{1,2})[Ee](\d{1,2})";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(filename);

            if (match.Success)
            {
                int.TryParse(match.Groups[1].Value, out int season);
                int.TryParse(match.Groups[2].Value, out int episode);
                return (season, episode);
            }

            return (null, null);
        }
    }
}
