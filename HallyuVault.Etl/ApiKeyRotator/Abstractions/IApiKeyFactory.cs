using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Core;

namespace HallyuVault.Etl.ApiKeyRotator.Abstractions
{
    public interface IApiKeyFactory<TKey> where TKey : ApiKey
    {
        ValueTask<TKey> CreateAsync(string apiKey);
    }
}
