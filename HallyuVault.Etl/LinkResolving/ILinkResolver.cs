using HallyuVault.Core.Abstractions;

namespace HallyuVault.Etl.LinkResolving
{
    public interface ILinkResolver
    {
        public Task<Result<string>> ResolveAsync(string link);
    }
}
