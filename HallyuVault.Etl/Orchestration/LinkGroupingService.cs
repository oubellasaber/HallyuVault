using HallyuVault.Etl.FileCryptExtractor.Entities.Rows;
using HallyuVault.Etl.FileCryptExtractor.Entities.Rows.Enums;
using HallyuVault.Etl.Models;

namespace HallyuVault.Etl.Orchestration
{
    public class LinkGroupingService
    {
        // Group Links
        public static Dictionary<string, List<FileCryptLink>> GroupLinks(List<FileCryptLink> links)
        {
            // Step 1: Filter active links
            var activeLinks = links
                .Where(link =>
                    !(link.Status == Status.Offline && link.FileName == null) &&
                    !(link.Status == Status.Unknown && new Uri(link.ContainerScrapedLink.ScrapedLink).Host != "dddrive.me")
                )
                .ToList();

            var activeNamedLinks = activeLinks
                .Where(link => link.FileName != null)
                .Select(x => x.FileName!)
                .Distinct()
                .ToList();

            // Step 2: Group by filename into a mutable dictionary
            var groupedByFilename = new Dictionary<string, List<FileCryptLink>>();
            var unnamedLinks = new List<FileCryptLink>();

            foreach (var row in activeLinks)
            {
                if (row.FileName != null)
                {
                    if (!groupedByFilename.ContainsKey(row.FileName))
                        groupedByFilename[row.FileName] = new List<FileCryptLink>();

                    groupedByFilename[row.FileName].Add(row);
                }
                else
                {
                    unnamedLinks.Add(row);
                }
            }

            int namedGroupsCount = groupedByFilename.Count;

            // Step 3: Group unnamed links by host
            var unnamedLinksGroupedByHost = unnamedLinks
                .GroupBy(link => new Uri(link.ContainerScrapedLink.ScrapedLink).Host)
                .ToList();

            // Step 4: Match unnamed groups to named groups by count
            foreach (var unnamedHostGroup in unnamedLinksGroupedByHost)
            {
                if (unnamedHostGroup.Count() == namedGroupsCount)
                {
                    var unnamedHostRows = unnamedHostGroup.ToList();

                    for (int i = 0; i < namedGroupsCount; i++)
                    {
                        var key = activeNamedLinks[i];
                        groupedByFilename[key!].Add(unnamedHostRows[i]);
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ Host group '{unnamedHostGroup.Key}' has {unnamedHostGroup.Count()} items, expected {namedGroupsCount}. Needs manual handling.");
                }
            }

            return groupedByFilename;
        }

    }
}