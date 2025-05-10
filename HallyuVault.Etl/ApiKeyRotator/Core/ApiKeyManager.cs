using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace HallyuVault.Etl.ApiKeyRotator.Core
{
    public abstract class ApiKeyManager<TKey, TSelectionArg> : IApiKeyManager<TKey, TSelectionArg> where TKey : ApiKey
    {
        private readonly IApiKeyFactory<TKey> _apiKeyFactory;
        private readonly IOptionsMonitor<ApiKeyRotationOptions> _optionsMonitor;
        private readonly ConcurrentDictionary<string, TKey> _apiKeys = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private bool _isInitialized;

        public IReadOnlyCollection<TKey> ApiKeys => _apiKeys.Values.ToImmutableList();

        public ApiKeyManager(
            IApiKeyFactory<TKey> apiKeyFactory,
            IOptionsMonitor<ApiKeyRotationOptions> optionsMonitor)
        {
            _apiKeyFactory = apiKeyFactory;
            _optionsMonitor = optionsMonitor;
            InitializeAsync().GetAwaiter().GetResult();
            _optionsMonitor.OnChange(async options => await HandleOptionsChangedAsync(options));
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            await _semaphore.WaitAsync();
            try
            {
                if (_isInitialized) return;

                var tasks = _optionsMonitor.CurrentValue.Keys.Select(AddAsync);
                await Task.WhenAll(tasks);
                _isInitialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<TKey> AddAsync(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API selectionArg cannot be empty", nameof(apiKey));

            var key = await _apiKeyFactory.CreateAsync(apiKey);
            _apiKeys.TryAdd(apiKey, key);

            return key;
        }

        public TKey? Get(TSelectionArg selectionArg)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Manager not initialized");

            var selected = SelectKey(selectionArg);
            return selected;
        }

        protected abstract TKey? SelectKey(TSelectionArg selectionArg);

        private async Task HandleOptionsChangedAsync(ApiKeyRotationOptions options)
        {
            await _semaphore.WaitAsync();
            try
            {
                var current = _apiKeys.Keys.ToHashSet();
                var incoming = options.Keys.ToHashSet();

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
