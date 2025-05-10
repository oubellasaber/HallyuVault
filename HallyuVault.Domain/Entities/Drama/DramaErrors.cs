using HallyuVault.Core.Abstractions;

namespace HallyuVault.Domain.Entities.Drama;

public class DramaErrors
{
    public static readonly Error NotFound = new(
        "Drama.NotFound",
        "The drama with the specified identifier was not found");
}
