using HallyuVault.Core.Abstractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.Domain.Abstractions
{
    public abstract class HtmlNodeParser<T> : Parser<HtmlNode, T>, IHtmlNodeParser<T>
    {
        protected HtmlNodeParser(IHtmlNodeValidator validator) : base(validator)
        {
        }
    }
}
