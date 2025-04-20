using HallyuVault.Core.Abstractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.DramaDayMediaParser.Abtractions
{
    public abstract class HtmlNodeParser<T> : Parser<HtmlNode, T>, IHtmlNodeParser<T>
    {
        protected HtmlNodeParser(IHtmlNodeValidator validator) : base(validator)
        {
        }
    }
}
