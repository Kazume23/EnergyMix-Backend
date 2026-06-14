using EnergyMix.Backend.Dtos.CarbonApi;

namespace EnergyMix.Backend.Calculators;

public interface ICleanEnergyCalculator
{
    decimal CalculateCleanEnergyPercentage(IEnumerable<GenerationMixItemDto> generationMix);
}
