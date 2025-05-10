using FakeItEasy;
using FluentAssertions;
using HallyuVault.Etl.ApiKeyRotator.Abstractions;
using HallyuVault.Etl.ApiKeyRotator.Core;
using HallyuVault.Etl.ApiKeyRotator.ScraperApi;
using Microsoft.Extensions.Options;

namespace HallyuVault.Etl.Tests.ApiKeyRotator.Tests;

public class ScraperApiKeyManager_CoreTests
{
    [Fact]
    public async Task InitializeAsync_Should_Add_All_Keys()
    {
        // Arrange
        var keys = new[] { "key1", "key2" };
        var options = new ApiKeyRotationOptions { Keys = keys };

        var monitor = A.Fake<IOptionsMonitor<ApiKeyRotationOptions>>();
        A.CallTo(() => monitor.CurrentValue).Returns(options);

        var factory = A.Fake<IApiKeyFactory<ScraperApiKey>>();
        A.CallTo(() => factory.CreateAsync(A<string>.Ignored))
            .ReturnsLazily(call =>
            {
                var key = call.GetArgument<string>(0)!;
                return ValueTask.FromResult(new ScraperApiKey(key, 1000, 100, 10, 1, DateTime.UtcNow));
            });

        var manager = new ScraperApiKeyManager(factory, monitor);

        // Act
        await manager.InitializeAsync();

        // Assert
        manager.ApiKeys.Should().HaveCount(2);
        manager.ApiKeys.Select(k => k.Key).Should().BeEquivalentTo("key1", "key2");
    }

    [Fact]
    public async Task AddAsync_Should_Throw_When_Key_Is_Empty()
    {
        // Arrange
        var monitor = A.Fake<IOptionsMonitor<ApiKeyRotationOptions>>();
        var factory = A.Fake<IApiKeyFactory<ScraperApiKey>>();
        var manager = new ScraperApiKeyManager(factory, monitor);

        // Act
        Func<Task> act = async () => await manager.AddAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void Get_Should_Throw_If_Not_Initialized()
    {
        // Arrange
        var monitor = A.Fake<IOptionsMonitor<ApiKeyRotationOptions>>();
        var factory = A.Fake<IApiKeyFactory<ScraperApiKey>>();
        var manager = new ScraperApiKeyManager(factory, monitor);

        // Act
        Action act = () => manager.Get(100);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
