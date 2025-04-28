using HallyuVault.Core.Abstractions;

namespace HallyuVault.Etl.LinkResolving
{
    public class LinkResolver : ILinkResolver
    {
        private readonly IEnumerable<ISpecializedLinkResolver> _linkResolvers;

        public LinkResolver(IEnumerable<ISpecializedLinkResolver> linkResolvers)
        {
            _linkResolvers = linkResolvers;
        }

        // ToDo. Add validators for links resolvers
        public async Task<Result<string>> ResolveAsync(string link)
        {
            foreach (var resolver in _linkResolvers)
            {
                var result = await resolver.ResolveAsync(link);
                if (result.IsSuccess)
                {
                    return result;
                }
            }

            return Result.Failure<string>(LinkResolvingErrors.NotSupported);
        }
    }
}
