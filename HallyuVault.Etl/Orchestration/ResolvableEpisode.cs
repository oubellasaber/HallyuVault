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

    public class LinkPair
    {
        public DramaDayLink RawLink { get; private set; }
        public ResolvedLink? ResolvedLink { get; private set; }
        public bool IsResolved => ResolvedLink != null;

        public LinkPair(DramaDayLink rawLink)
        {
            RawLink = rawLink;
        }

        public void SetResolvedLink(ResolvedLink resolvedLink)
        {
            if (ResolvedLink is null)
            {
                ResolvedLink = resolvedLink;
            }
        }
    }


    public class ResolvableEpisode
    {
        public int SeasonNumber { get; }
        public string MediaVersionName { get; }
        public EpisodeType Type { get; }
        public Range? EpisodeRange { get; }
        public string EpisodeVersionName { get; }
        public IEnumerable<LinkPair> Links { get; }

        public ResolvableEpisode(
            int seasonNumber,
            string mediaVersionName,
            EpisodeType type,
            Range? episodeRange,
            string episodeVersionName,
            IEnumerable<LinkPair> links)
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
                BatchEpisode batchEpisode => (EpisodeType.Ranged, new Range(batchEpisode.EpisodeStart, batchEpisode.EpisodeEnd)),
                SpecialEpisode specialEpisode => (EpisodeType.Special, null),
                _ => throw new UnreachableException($"Unknown episode type: {episode.GetType().Name}.")
            };

            (Type, EpisodeRange) = episodeInfo;

            SeasonNumber = season.SeasonNumber ?? 1;
            MediaVersionName = mediaVersion.Name;
            EpisodeVersionName = episodeVersion.Name;
            Links = episodeVersion.Links.Select(link => new LinkPair(link)).ToList();
        }
    }
}
