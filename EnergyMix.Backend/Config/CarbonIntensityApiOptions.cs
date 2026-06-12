namespace EnergyMix.Backend.Config;

public sealed class CarbonIntensityApiOptions
{
    public const string SectionName = "CarbonIntensityApi";

    public string BaseUrl { get; init; } = string.Empty;
    public int TotalTimeoutSeconds { get; init; } = 30;
    public int AttemptTimeoutSeconds { get; init; } = 8;
    public int RetryCount { get; init; } = 2;
    public int RetryDelayMilliseconds { get; init; } = 500;
}
