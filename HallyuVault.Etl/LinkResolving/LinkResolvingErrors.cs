using HallyuVault.Core.Abstractions;

namespace HallyuVault.Etl.LinkResolving
{
    public static class LinkResolvingErrors
    {
        public static readonly Error NoRedirect = new Error("LinkResolvingError.NoRedirect", "No redirect found");
        public static readonly Error NotSupported = new Error("LinkResolvingError.NotSupported", "No resolver support such a link");
    }
}
