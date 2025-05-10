using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Core;

namespace HallyuVault.Etl.ApiKeyRotator.Abstractions
{
    public interface IApiKeyManager<TKey, TSelectionArg> where TKey : ApiKey
    {
        Task<TKey> AddAsync(string apiKey);
        TKey? Get(TSelectionArg key);
    }
}
