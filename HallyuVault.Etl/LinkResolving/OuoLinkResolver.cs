using HallyuVault.Core.Abstractions;

namespace HallyuVault.Etl.LinkResolving
{
    public class OuoLinkResolver : ILinkResolver
    {
        public Task<Result<string>> ResolveAsync(string link)
        {
            throw new NotImplementedException();
        }
    }
}
