using HallyuVault.Core.Abstractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.Abtractions
{
    public interface IHtmlNodeParser<T> : IParser<HtmlNode, T>;
}
