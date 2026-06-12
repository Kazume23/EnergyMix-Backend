using EnergyMix.Backend.Models;

namespace EnergyMix.Backend.Services;

public sealed class CarbonService
{
    private readonly CarbonIntensityService _carbonIntensityService;
    private readonly EnergyMixCalculator _energyMixCalculator;
    private readonly ChargingWindowCalculator _chargingWindowCalculator;

    public CarbonService(
        CarbonIntensityService carbonIntensityService,
        EnergyMixCalculator energyMixCalculator,
        ChargingWindowCalculator chargingWindowCalculator)
    {
        _carbonIntensityService = carbonIntensityService;
        _energyMixCalculator = energyMixCalculator;
        _chargingWindowCalculator = chargingWindowCalculator;
    }

    public async Task<List<DailyEnergyMixResponse>> GetDailyMixAsync()
    {
        var startDateUtc = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(3);

        var generationResponse = await _carbonIntensityService.GetGenerationAsync(
            startDateUtc,
            endDateUtc);

        var requestedIntervals = generationResponse.Data
            .Where(generationInterval =>
                generationInterval.From >= startDateUtc &&
                generationInterval.To <= endDateUtc)
            .ToList();

        return _energyMixCalculator.CalculateDailyEnergyMix(requestedIntervals);
    }

    public async Task<OptimalChargingWindowResponse> GetOptimalChargingWindowAsync(int hours)
    {
        var startDateUtc = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(2);

        var generationResponse = await _carbonIntensityService.GetGenerationAsync(
            startDateUtc,
            endDateUtc);

        return _chargingWindowCalculator.FindOptimalChargingWindow(
            generationResponse.Data,
            hours);
    }
}
