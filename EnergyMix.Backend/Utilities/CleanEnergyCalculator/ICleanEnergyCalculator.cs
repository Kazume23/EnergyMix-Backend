using EnergyMix.Backend.Dtos.CarbonApi;

namespace EnergyMix.Backend.Utilities;

public interface ICleanEnergyCalculator
{
    decimal CalculateCleanEnergyPercentage(IEnumerable<GenerationMixItemDto> generationMix);
}
