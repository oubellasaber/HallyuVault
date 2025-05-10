using FakeItEasy;
using FluentAssertions;
using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Core;
using HallyuVault.Etl.ApiKeyRotator.ScraperApi;
using Microsoft.Extensions.Options;

namespace HallyuVault.Etl.Tests.ApiKeyRotator.Tests;

public class ScraperApiKeyManager_SelectionTests
{
    [Fact]
    public async Task SelectKey_Should_Return_Key_With_Sufficient_Credits_And_Low_Concurrency()
    {
        // Arrange
        var factory = A.Fake<IApiKeyFactory<ScraperApiKey>>();
        var monitor = A.Fake<IOptionsMonitor<ApiKeyRotationOptions>>();
        var options = new ApiKeyRotationOptions
        {
            Keys = new string[] { "key1", "key2", "key3" }
        };

        A.CallTo(() => monitor.CurrentValue).Returns(options);

        var key1 = new ScraperApiKey("key1", totalCredits: 1000, consumedCredits: 900, concurrencyLimit: 5, concurrentRequests: 1, DateTime.UtcNow);
        var key2 = new ScraperApiKey("key2", totalCredits: 1000, consumedCredits: 100, concurrencyLimit: 5, concurrentRequests: 5, DateTime.UtcNow); // over limit
        var key3 = new ScraperApiKey("key3", totalCredits: 1000, consumedCredits: 50, concurrencyLimit: 5, concurrentRequests: 1, DateTime.UtcNow);

        A.CallTo(() => factory.CreateAsync("key1")).Returns(key1);
        A.CallTo(() => factory.CreateAsync("key2")).Returns(key2);
        A.CallTo(() => factory.CreateAsync("key3")).Returns(key3);

        var manager = new ScraperApiKeyManager(factory, monitor);

        // Initialize the manager to populate the keys
        await manager.InitializeAsync();

        // Act
        var selected = manager.Get(50);

        // Assert
        selected.Should().Be(key3); // has more available credits than key1 and isn't over concurrency
    }

    [Fact]
    public async Task SelectKey_Should_Return_Null_If_No_Key_Matches()
    {
        // Arrange
        var factory = A.Fake<IApiKeyFactory<ScraperApiKey>>();
        var monitor = A.Fake<IOptionsMonitor<ApiKeyRotationOptions>>();
        var options = new ApiKeyRotationOptions
        {
            Keys = new string[] { "key1" }
        };

        A.CallTo(() => monitor.CurrentValue).Returns(options);

        var key = new ScraperApiKey("key1", totalCredits: 100, consumedCredits: 100, concurrencyLimit: 1, concurrentRequests: 1, DateTime.UtcNow);
        A.CallTo(() => factory.CreateAsync("key1")).Returns(key);

        var manager = new ScraperApiKeyManager(factory, monitor);

        // Initialize the manager to populate the keys
        await manager.InitializeAsync();

        // Act
        var selected = manager.Get(1);

        // Assert
        selected.Should().BeNull();
    }
}