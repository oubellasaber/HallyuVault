using HallyuVault.Core.Abstractions;
using HtmlAgilityPack;

namespace HallyuVault.Etl.Domain.Abstractions
{
    public interface IHtmlNodeValidator : IValidator<HtmlNode>;
    public interface IHtmlNodeValidator<TFor> : IHtmlNodeValidator;
}
