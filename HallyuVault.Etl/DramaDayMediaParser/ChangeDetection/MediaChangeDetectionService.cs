using HallyuVault.Etl.DramaDayMediaParser.SeasonParsing;

namespace HallyuVault.Etl.DramaDayMediaParser.ChangeDetection
{
    public class MediaChangeDetectionService
    {
        public static List<Season> DetectChanges(
            List<Season> oldSeasons,
            List<Season> newSeasons)
        {
            var changes = new List<Season>();
            foreach (var newSeason in newSeasons)
            {
                var oldSeason = oldSeasons.FirstOrDefault(s => s.SeasonNumber == newSeason.SeasonNumber);

                if (oldSeason == null)
                {
                    changes.Add(newSeason);
                    continue;
                }

                // Compare media versions
                var newMediaVersions = newSeason.MediaVersions.Where(newMedia =>
                    oldSeason.MediaVersions.All(oldMedia => oldMedia.Name != newMedia.Name)).ToList();

                if (!newMediaVersions.Any())
                    continue;

                foreach (var newMediaVersion in newMediaVersions)
                {
                    var newEpisodes = newMediaVersion.Episodes.Where(newEpisode =>
                        oldSeason.MediaVersions.SelectMany(m => m.Episodes)
                            .All(oldEpisode => oldEpisode.Id != newEpisode.Id)).ToList();

                    if (!newEpisodes.Any())
                        continue;

                    foreach (var newEpisode in newEpisodes)
                    {
                        var newEpisodeVersions = newEpisode.EpisodeVersions.Where(newVersion =>
                            oldSeason.MediaVersions.SelectMany(m => m.Episodes)
                                .SelectMany(e => e.EpisodeVersions)
                                .All(oldVersion => oldVersion.Name != newVersion.Name)).ToList();

                        if (!newEpisodeVersions.Any())
                            continue;

                        foreach (var newVersion in newEpisodeVersions)
                        {
                            var newLinks = newVersion.Links.Where(newLink =>
                                oldSeason.MediaVersions.SelectMany(m => m.Episodes)
                                    .SelectMany(e => e.EpisodeVersions)
                                    .SelectMany(v => v.Links)
                                    .All(oldLink => oldLink.Url != newLink.Url)).ToList();

                            if (!newLinks.Any())
                                continue;

                            newVersion.AddLinkRange(newLinks);
                        }
                    }
                }
            }

            return changes;
        }
    }
}