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
        decimal cleanEnergyPercentage = 0m;

        foreach(var generationMixItem in generationMix)
        {
            if (CleanEnergyTypes.Contains(generationMixItem.Fuel))
            {
                cleanEnergyPercentage += generationMixItem.Percentage;
            }
        }

        return cleanEnergyPercentage;
    }
}