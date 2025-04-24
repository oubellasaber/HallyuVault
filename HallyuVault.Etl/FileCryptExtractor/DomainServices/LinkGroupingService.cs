using HallyuVault.Etl.FileCryptExtractor.Entities.Rows;
using HallyuVault.Etl.FileCryptExtractor.Entities.Rows.Enums;

namespace HallyuVault.Etl.FileCryptExtractor.DomainServices
{
    public class LinkGroupingService
    {
        // Group Links
        public static Dictionary<string, List<Row>> GroupLinks(List<Row> rows)
        {
            // Step 1: Filter active links
            var activeLinks = rows
                .Where(row =>
                    !(row.Link.Status == Status.Offline && row.FileName == null) &&
                    !(row.Link.Status == Status.Unknown && row.Link.Url.Host != "dddrive.me")
                )
                .ToList();

            var activeNamedLinks = activeLinks
                .Where(row => row.FileName != null)
                .Select(x => x.FileName!)
                .Distinct()
                .ToList();

            // Step 2: Group by filename into a mutable dictionary
            var groupedByFilename = new Dictionary<string, List<Row>>();
            var unnamedLinks = new List<Row>();

            foreach (var row in activeLinks)
            {
                if (row.FileName != null)
                {
                    if (!groupedByFilename.ContainsKey(row.FileName))
                        groupedByFilename[row.FileName] = new List<Row>();

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
                .GroupBy(row => row.Link.Url.Host)
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