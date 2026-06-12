namespace EnergyMix.Backend.Config;

public sealed class CarbonIntensityApiOptions
{
    public const string SectionName = "CarbonIntensityApi";

    public string BaseUrl { get; init; } = string.Empty;
    public int TotalTimeoutSeconds { get; init; } = 45;
    public int AttemptTimeoutSeconds { get; init; } = 20;
    public int RetryCount { get; init; } = 1;
    public int RetryDelayMilliseconds { get; init; } = 1000;
    public int CircuitBreakerSamplingDurationSeconds { get; init; } = 60;
}
