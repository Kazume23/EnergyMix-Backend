using EnergyMix.Backend.Dtos.CarbonApi;

namespace EnergyMix.Backend.Calculators;

public sealed class CleanEnergyCalculator : ICleanEnergyCalculator
{
    private static readonly HashSet<string> CleanEnergyTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "biomass",
        "hydro",
        "nuclear",
        "solar",
        "wind"
    };

    public decimal CalculateCleanEnergyPercentage(IEnumerable<GenerationMixItemDto> generationMix)
    {
        return generationMix
            .Where(generationMixItem => CleanEnergyTypes.Contains(generationMixItem.Fuel))
            .Sum(generationMixItem => generationMixItem.Percentage);
    }
}
