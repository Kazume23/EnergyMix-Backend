using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;

namespace EnergyMix.Backend.Utilities;

public interface IChargingWindowCalculator
{
    OptimalChargingWindowResponseDto FindOptimalChargingWindow(
        IEnumerable<GenerationIntervalDto> generationIntervals,
        int hours);
}
