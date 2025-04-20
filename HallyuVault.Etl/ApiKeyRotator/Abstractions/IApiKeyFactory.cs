using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Core;

namespace HallyuVault.Etl.ApiKeyRotator.Abstractions
{
    public interface IApiKeyFactory<T> where T : ApiKey
    {
        ValueTask<Result<T>> CreateAsync(string apiKey);
    }
}
