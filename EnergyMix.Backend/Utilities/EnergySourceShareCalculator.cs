using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;

namespace EnergyMix.Backend.Utilities;

public static class EnergySourceShareCalculator
{
    public static List<EnergySourceShareResponseDto> CalculateAverageSourceShares(
        IEnumerable<GenerationIntervalDto> generationIntervals)
    {
        return generationIntervals
            .SelectMany(generationInterval => generationInterval.GenerationMix)
            .GroupBy(generationMixItem => generationMixItem.Fuel, StringComparer.OrdinalIgnoreCase)
            .OrderBy(sourceGroup => sourceGroup.Key, StringComparer.OrdinalIgnoreCase)
            .Select(sourceGroup => new EnergySourceShareResponseDto
            {
                Fuel = sourceGroup.Key,
                Percentage = decimal.Round(sourceGroup.Average(generationMixItem => generationMixItem.Percentage), 2)
            })
            .ToList();
    }
}
