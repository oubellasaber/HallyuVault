using HallyuVault.Core.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.ScraperApi;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace HallyuVault.Etl.ApiKeyRotator.Core
{
    public abstract class ApiKeyManager<T> : IApiKeyManager<T> where T : ApiKey
    {
        private readonly IApiKeyFactory<T> _apiKeyFactory;
        private readonly IOptionsMonitor<ApiKeyRotationOptions> _optionsMonitor;
        private readonly ConcurrentDictionary<string, T> _apiKeys = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _isInitialized;

        public IReadOnlyCollection<T> ApiKeys => _apiKeys.Values.ToImmutableList();

        public ApiKeyManager(
            IApiKeyFactory<T> apiKeyFactory,
            IOptionsMonitor<ApiKeyRotationOptions> optionsMonitor)
        {
            _apiKeyFactory = apiKeyFactory;
            _optionsMonitor = optionsMonitor;
            _optionsMonitor.OnChange(async options => await HandleOptionsChangedAsync(options));
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            await _semaphore.WaitAsync();
            try
            {
                if (_isInitialized) return;

                var tasks = _optionsMonitor.CurrentValue.ApiKeys.Select(AddAsync);
                await Task.WhenAll(tasks);
                _isInitialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Result<T>> AddAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return Result.Failure<T>(new Error("EmptyKey", "API key cannot be null or empty"));

            var result = await _apiKeyFactory.CreateAsync(apiKey);
            if (result.IsSuccess)
                _apiKeys[apiKey] = result.Value;

            return result;
        }

        public Result<T> Get(int estimitedCredits)
        {
            if (!_isInitialized)
                return Result.Failure<T>(new Error("NotInitialized", "Manager not initialized")); // exception

            var selected = SelectKey(estimitedCredits);
            // id null is not
            return Result.Success(selected);
        }

        protected abstract T SelectKey(int estimitedCredits);

        private async Task HandleOptionsChangedAsync(ApiKeyRotationOptions options)
        {
            await _semaphore.WaitAsync();
            try
            {
                var current = _apiKeys.Keys.ToHashSet();
                var incoming = options.ApiKeys.ToHashSet();

                var toAdd = incoming.Except(current);
                var addTasks = toAdd.Select(AddAsync);
                await Task.WhenAll(addTasks);

                var toRemove = current.Except(incoming);
                foreach (var key in toRemove)
                    _apiKeys.TryRemove(key, out _);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
