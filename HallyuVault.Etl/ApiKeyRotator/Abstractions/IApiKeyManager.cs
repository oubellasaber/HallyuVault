using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Core;

namespace HallyuVault.Etl.ApiKeyRotator.Abstractions
{
    public interface IApiKeyManager<T> where T : ApiKey
    {
        Task<Result<T>> AddAsync(string apiKey);
        Result<T> Get(int estimitedCredits);
    }
}
