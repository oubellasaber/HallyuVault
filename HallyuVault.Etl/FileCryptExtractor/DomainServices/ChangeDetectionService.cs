using HallyuVault.Etl.FileCryptExtractor.Entities.Rows;

namespace HallyuVault.Etl.FileCryptExtractor.DomainServices
{
    public class ChangeDetectionService
    {
        public Dictionary<string, List<Row>> DetectChanges(
            Dictionary<string, List<Row>> oldRows,
            Dictionary<string, List<Row>> newRows)
        {
            var result = new Dictionary<string, List<Row>>(newRows.Count);

            foreach (var (filename, newGroup) in newRows)
            {
                if (!oldRows.TryGetValue(filename, out var oldGroup))
                {
                    // Entirely new group
                    result[filename] = newGroup;
                    continue;
                }

                // A group that already exist but has new links
                var newOnly = newGroup.ExceptBy(oldGroup.Select(r => r.Link.Url), r => r.Link.Url).ToList();

                if (newOnly.Count > 0)
                {
                    result.Add(filename, newOnly);
                }
            }

            return result;
        }
    }
}