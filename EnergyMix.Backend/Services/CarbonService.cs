using EnergyMix.Backend.Calculators;
using EnergyMix.Backend.Clients;
using EnergyMix.Backend.Constants;
using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;
using Microsoft.Extensions.Caching.Memory;

namespace EnergyMix.Backend.Services;

public sealed class CarbonService : ICarbonService
{
    private static readonly TimeSpan GenerationCacheDuration = TimeSpan.FromMinutes(30);

    private readonly ICarbonIntensityClient _carbonIntensityClient;
    private readonly IChargingWindowCalculator _chargingWindowCalculator;
    private readonly IEnergyMixCalculator _energyMixCalculator;
    private readonly ILogger<CarbonService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly TimeProvider _timeProvider;

    public CarbonService(
        ICarbonIntensityClient carbonIntensityClient,
        IChargingWindowCalculator chargingWindowCalculator,
        IEnergyMixCalculator energyMixCalculator,
        ILogger<CarbonService> logger,
        IMemoryCache memoryCache,
        TimeProvider timeProvider)
    {
        _carbonIntensityClient = carbonIntensityClient;
        _chargingWindowCalculator = chargingWindowCalculator;
        _energyMixCalculator = energyMixCalculator;
        _logger = logger;
        _memoryCache = memoryCache;
        _timeProvider = timeProvider;
    }

    public async Task<List<DailyEnergyMixResponseDto>> GetDailyMixAsync(
        CancellationToken cancellationToken = default)
    {
        var startDateUtc = new DateTimeOffset(_timeProvider.GetUtcNow().Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(EnergyMixConstants.DailyMixDaysAhead);

        var generationResponse = await GetCachedGenerationAsync(
            startDateUtc,
            endDateUtc,
            cancellationToken);

        var requestedIntervals = generationResponse.Data
            .Where(generationInterval =>
                generationInterval.From >= startDateUtc &&
                generationInterval.To <= endDateUtc)
            .ToList();

        return _energyMixCalculator.CalculateDailyEnergyMix(requestedIntervals);
    }

    public async Task<OptimalChargingWindowResponseDto> GetOptimalChargingWindowAsync(
        int hours,
        CancellationToken cancellationToken = default)
    {
        var startDateUtc = new DateTimeOffset(_timeProvider.GetUtcNow().Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(EnergyMixConstants.ChargingWindowDaysAhead);

        var generationResponse = await GetCachedGenerationAsync(
            startDateUtc,
            endDateUtc,
            cancellationToken);

        return _chargingWindowCalculator.FindOptimalChargingWindow(
            generationResponse.Data,
            hours);
    }

    private async Task<CarbonGenerationResponseDto> GetCachedGenerationAsync(
        DateTimeOffset startDateUtc,
        DateTimeOffset endDateUtc,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"generation:{startDateUtc:O}:{endDateUtc:O}";

        if (_memoryCache.TryGetValue(cacheKey, out CarbonGenerationResponseDto? cachedGeneration) &&
            cachedGeneration is not null)
        {
            _logger.LogInformation(
                "Using cached Carbon Intensity data for {StartDateUtc} - {EndDateUtc}.",
                startDateUtc,
                endDateUtc);

            return cachedGeneration;
        }

        _logger.LogInformation(
            "Fetching Carbon Intensity data for {StartDateUtc} - {EndDateUtc}.",
            startDateUtc,
            endDateUtc);

        var generationResponse = await _carbonIntensityClient.GetGenerationAsync(
            startDateUtc,
            endDateUtc,
            cancellationToken);

        _memoryCache.Set(cacheKey, generationResponse, GenerationCacheDuration);

        return generationResponse;
    }
}
