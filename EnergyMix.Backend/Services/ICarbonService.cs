using EnergyMix.Backend.Dtos.Responses;

namespace EnergyMix.Backend.Services;

public interface ICarbonService
{
    Task<List<DailyEnergyMixResponseDto>> GetDailyMixAsync(CancellationToken cancellationToken = default);

    Task<OptimalChargingWindowResponseDto> GetOptimalChargingWindowAsync(
        int hours,
        CancellationToken cancellationToken = default);
}
