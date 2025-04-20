using HallyuVault.Core.Abstractions;

namespace HallyuVault.Etl.LinkResolving
{
    public static class LinkResolvingErrors
    {
        public static readonly Error NoRedirect = new Error("LinkResolvingError.NoRedirect", "No redirect found");
    }
}
