using FluentAssertions;
using HallyuVault.Etl.ApiKeyRotator.ScraperApi;

namespace HallyuVault.Etl.Tests.ApiKeyRotator.Tests;

public class ScraperApiKey_CreditUpdateTests
{
    [Fact]
    public void UpdateConsumedCredits_ByPositive_IncrementsConsumedAndConcurrentRequests()
    {
        // Arrange
        var initialConsumed = 10;
        var initialConcurrent = 2;
        var apiKey = new ScraperApiKey(
            apiKey: "test",
            totalCredits: 100,
            consumedCredits: initialConsumed,
            concurrencyLimit: 5,
            concurrentRequests: initialConcurrent,
            subscriptionDate: DateTime.UtcNow);

        // Act
        apiKey.UpdateConsumedCredits(3);

        // Assert
        apiKey.ConsumedCredits.Should().Be(initialConsumed + 3);
        apiKey.ConcurrentRequests.Should().Be(initialConcurrent + 1);
    }

    [Fact]
    public void UpdateConsumedCredits_ByNonPositive_DecrementsConsumedAndConcurrentRequests()
    {
        // Arrange
        var initialConsumed = 20;
        var initialConcurrent = 4;
        var apiKey = new ScraperApiKey(
            apiKey: "test",
            totalCredits: 100,
            consumedCredits: initialConsumed,
            concurrencyLimit: 5,
            concurrentRequests: initialConcurrent,
            subscriptionDate: DateTime.UtcNow);

        // Act
        apiKey.UpdateConsumedCredits(-5);

        // Assert
        // consumedCredits = initialConsumed - (-5) = initialConsumed + 5
        apiKey.ConsumedCredits.Should().Be(initialConsumed + 5);
        apiKey.ConcurrentRequests.Should().Be(initialConcurrent - 1);
    }

    [Fact]
    public void UpdateConsumedCredits_DecAndInc_CalculatesCorrectly_And_DecrementsConcurrentRequests()
    {
        // Arrange
        var initialConsumed = 50;
        var initialConcurrent = 3;
        var apiKey = new ScraperApiKey(
            apiKey: "test",
            totalCredits: 200,
            consumedCredits: initialConsumed,
            concurrencyLimit: 10,
            concurrentRequests: initialConcurrent,
            subscriptionDate: DateTime.UtcNow);

        // Act
        // dec = 4, inc = 7 → consumed = 50 - 4 + 7 = 53, concurrentRequests = 3 - 1 = 2
        apiKey.UpdateConsumedCredits(dec: 4, inc: 7);

        // Assert
        apiKey.ConsumedCredits.Should().Be(initialConsumed - 4 + 7);
        apiKey.ConcurrentRequests.Should().Be(initialConcurrent - 1);
    }

    [Fact]
    public void AvailableCredits_IsTotalMinusConsumed()
    {
        // Arrange
        var total = 150;
        var used = 47;
        var apiKey = new ScraperApiKey(
            apiKey: "unused",
            totalCredits: total,
            consumedCredits: used,
            concurrencyLimit: 5,
            concurrentRequests: 0,
            subscriptionDate: DateTime.UtcNow);

        // Act
        var available = apiKey.AvailableCredits;

        // Assert
        available.Should().Be(total - used);
    }
}