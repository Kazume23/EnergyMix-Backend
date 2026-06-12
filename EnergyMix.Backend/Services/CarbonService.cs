using EnergyMix.Backend.Calculators;
using EnergyMix.Backend.Clients;
using EnergyMix.Backend.Dtos.Responses;

namespace EnergyMix.Backend.Services;

public sealed class CarbonService
{
    private readonly CarbonIntensityClient _carbonIntensityClient;

    public CarbonService(CarbonIntensityClient carbonIntensityClient)
    {
        _carbonIntensityClient = carbonIntensityClient;
    }

    public async Task<List<DailyEnergyMixResponseDto>> GetDailyMixAsync()
    {
        var startDateUtc = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(3);

        var generationResponse = await _carbonIntensityClient.GetGenerationAsync(
            startDateUtc,
            endDateUtc);

        var requestedIntervals = generationResponse.Data
            .Where(generationInterval =>
                generationInterval.From >= startDateUtc &&
                generationInterval.To <= endDateUtc)
            .ToList();

        return EnergyMixCalculator.CalculateDailyEnergyMix(requestedIntervals);
    }

    public async Task<OptimalChargingWindowResponseDto> GetOptimalChargingWindowAsync(int hours)
    {
        var startDateUtc = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(2);

        var generationResponse = await _carbonIntensityClient.GetGenerationAsync(
            startDateUtc,
            endDateUtc);

        return ChargingWindowCalculator.FindOptimalChargingWindow(
            generationResponse.Data,
            hours);
    }
}
