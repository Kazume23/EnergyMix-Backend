using System.Text.Json;
using Xunit;

namespace EnergyMix.Backend.Tests.Config;

public class CarbonIntensityApiConfigurationTests
{
    [Fact]
    public void Appsettings_ContainsValidCarbonIntensityApiTimeoutConfiguration()
    {
        using var appsettingsDocument = JsonDocument.Parse(
            File.ReadAllText(GetBackendAppsettingsPath()));
        var carbonIntensityApiSection = appsettingsDocument
            .RootElement
            .GetProperty("CarbonIntensityApi");

        var totalTimeoutSeconds = carbonIntensityApiSection
            .GetProperty("TotalTimeoutSeconds")
            .GetInt32();
        var attemptTimeoutSeconds = carbonIntensityApiSection
            .GetProperty("AttemptTimeoutSeconds")
            .GetInt32();
        var retryCount = carbonIntensityApiSection
            .GetProperty("RetryCount")
            .GetInt32();
        var retryDelayMilliseconds = carbonIntensityApiSection
            .GetProperty("RetryDelayMilliseconds")
            .GetInt32();
        var circuitBreakerSamplingDurationSeconds = carbonIntensityApiSection
            .GetProperty("CircuitBreakerSamplingDurationSeconds")
            .GetInt32();

        Assert.Equal(45, totalTimeoutSeconds);
        Assert.Equal(20, attemptTimeoutSeconds);
        Assert.Equal(1, retryCount);
        Assert.Equal(1000, retryDelayMilliseconds);
        Assert.Equal(60, circuitBreakerSamplingDurationSeconds);
        Assert.True(
            circuitBreakerSamplingDurationSeconds >= attemptTimeoutSeconds * 2,
            "Circuit breaker sampling duration must be at least double the attempt timeout.");
        Assert.True(
            totalTimeoutSeconds >= (attemptTimeoutSeconds * (retryCount + 1)) + (retryDelayMilliseconds / 1000),
            "Total timeout must cover all retry attempts and retry delays.");
    }

    private static string GetBackendAppsettingsPath()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            ".."));

        return Path.Combine(repositoryRoot, "EnergyMix.Backend", "appsettings.json");
    }
}
