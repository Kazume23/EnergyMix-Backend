using EnergyMix.Backend.Models;

namespace EnergyMix.Backend.Services;

public sealed class CleanEnergyCalculator
{
    private static readonly HashSet<string> CleanEnergyTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "biomass",
        "hydro",
        "nuclear",
        "solar",
        "wind"
    };

    public decimal CalculateCleanEnergyPercentage(IEnumerable<GenerationMixItem> generationMix)
    {
        return generationMix
            .Where(generationMixItem => CleanEnergyTypes.Contains(generationMixItem.Fuel))
            .Sum(generationMixItem => generationMixItem.Percentage);
    }
}
