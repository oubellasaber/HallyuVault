using HallyuVault.Core.Abstractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.Domain.Abstractions
{
    public interface IHtmlNodeParser<T> : IParser<HtmlNode, T>;
}
