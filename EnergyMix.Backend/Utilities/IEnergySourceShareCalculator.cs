using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;

namespace EnergyMix.Backend.Utilities;

public interface IEnergySourceShareCalculator
{
    List<EnergySourceShareResponseDto> CalculateAverageSourceShares(
        IEnumerable<GenerationIntervalDto> generationIntervals);
}
