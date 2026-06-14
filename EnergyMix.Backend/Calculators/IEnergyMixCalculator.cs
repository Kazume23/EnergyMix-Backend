using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;

namespace EnergyMix.Backend.Calculators;

public interface IEnergyMixCalculator
{
    List<DailyEnergyMixResponseDto> CalculateDailyEnergyMix(
        IEnumerable<GenerationIntervalDto> generationIntervals);
}
