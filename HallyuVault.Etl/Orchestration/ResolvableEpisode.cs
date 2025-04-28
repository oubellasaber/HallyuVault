using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing;
using HallyuVault.Etl.Models;
using System.Diagnostics;

namespace HallyuVault.Etl.Orchestration
{
    public enum EpisodeType
    {
        Standard,
        Ranged,
        Special
    }

    public class ResolvableEpisode
    {
        public int SeasonNumber { get; }
        public string MediaVersionName { get; }
        public EpisodeType Type { get; }
        public Range? EpisodeRange { get; }
        public string EpisodeVersionName { get; }
        public IEnumerable<(DramaDayLink RawLink, ResolvedLink? ResolvedLink)> Links { get; }

        public ResolvableEpisode(
            int seasonNumber,
            string mediaVersionName,
            EpisodeType type,
            Range? episodeRange,
            string episodeVersionName,
            IEnumerable<(DramaDayLink, ResolvedLink?)> links)
        {
            SeasonNumber = seasonNumber;
            MediaVersionName = mediaVersionName;
            Type = type;
            EpisodeRange = episodeRange;
            EpisodeVersionName = episodeVersionName;
            Links = links;
        }

        public ResolvableEpisode(
            Season season,
            MediaVersion mediaVersion,
            Episode episode,
            EpisodeVersion episodeVersion)
        {
            (EpisodeType, Range?) episodeInfo = episode switch
            {
                StandardEpisode standardEpisode => (EpisodeType.Standard, new Range(standardEpisode.EpisodeNumber, standardEpisode.EpisodeNumber)),
                BatchEpisode batchEpisode => (EpisodeType.Ranged, batchEpisode.EpisodeRange),
                SpecialEpisode specialEpisode => (EpisodeType.Special, null),
                _ => throw new UnreachableException($"Unknown episode type: {episode.GetType().Name}.")
            };

            (Type, EpisodeRange) = episodeInfo;

            SeasonNumber = season.SeasonNumber ?? 1;
            MediaVersionName = mediaVersion.Name;
            EpisodeVersionName = episodeVersion.Name;
            Links = episodeVersion.Links.Select<DramaDayLink, (DramaDayLink, ResolvedLink?)>(link => (link, null));

        }
    }
}
