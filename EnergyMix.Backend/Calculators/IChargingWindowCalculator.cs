using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;

namespace EnergyMix.Backend.Calculators;

public interface IChargingWindowCalculator
{
    OptimalChargingWindowResponseDto FindOptimalChargingWindow(
        IEnumerable<GenerationIntervalDto> generationIntervals,
        int hours);
}
