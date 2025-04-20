using HtmlAgilityPack;

namespace HallyuVault.Core.Abstractions
{
    public static class HtmlNodeExtensions
    {
        static List<List<HtmlNode>> SplitByNode(this HtmlNode parentNode, string name)
        {
            var groups = new List<List<HtmlNode>>();
            var currentGroup = new List<HtmlNode>();

            foreach (var node in parentNode.ChildNodes)
            {
                if (node.Name == "br")
                {
                    if (currentGroup.Count > 0)
                    {
                        groups.Add(currentGroup);
                        currentGroup = new List<HtmlNode>();
                    }
                }
                else
                {
                    currentGroup.Add(node);
                }
            }

            // Add last group if not empty
            if (currentGroup.Count > 0)
                groups.Add(currentGroup);

            return groups;
        }

        static List<List<HtmlNode>> SplitByBr(this HtmlNode node)
        {
            return node.SplitByNode("br");
        }

        public static string GetAttributeValue(this HtmlNode node, string name)
        {
            var attributeValue = node.GetAttributeValue(name, string.Empty);

            if (string.IsNullOrEmpty(attributeValue))
            {
                throw new NodeAttributeNotFoundException(name);
            }

            return attributeValue;
        }
    }
}
