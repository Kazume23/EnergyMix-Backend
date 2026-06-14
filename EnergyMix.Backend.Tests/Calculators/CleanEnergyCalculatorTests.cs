using EnergyMix.Backend.Calculators;
using EnergyMix.Backend.Tests.Helpers;
using Xunit;

namespace EnergyMix.Backend.Tests.Calculators;

public class CleanEnergyCalculatorTests
{
    [Fact]
    public void CalculateCleanEnergyPercentage_ReturnsSumOfOnlyCleanEnergySources()
    {
        var generationMix = new[]
        {
            GenerationTestDataBuilder.MixItem("biomass", 10m),
            GenerationTestDataBuilder.MixItem("nuclear", 20m),
            GenerationTestDataBuilder.MixItem("hydro", 5m),
            GenerationTestDataBuilder.MixItem("wind", 15m),
            GenerationTestDataBuilder.MixItem("solar", 7m),
            GenerationTestDataBuilder.MixItem("gas", 30m),
            GenerationTestDataBuilder.MixItem("coal", 13m)
        };

        var cleanEnergyCalculator = new CleanEnergyCalculator();

        var cleanEnergyPercentage = cleanEnergyCalculator.CalculateCleanEnergyPercentage(generationMix);

        Assert.Equal(57m, cleanEnergyPercentage);
    }

    [Fact]
    public void CalculateCleanEnergyPercentage_ReturnsZero_WhenThereAreNoCleanEnergySources()
    {
        var generationMix = new[]
        {
            GenerationTestDataBuilder.MixItem("gas", 60m),
            GenerationTestDataBuilder.MixItem("coal", 20m),
            GenerationTestDataBuilder.MixItem("imports", 20m)
        };

        var cleanEnergyCalculator = new CleanEnergyCalculator();

        var cleanEnergyPercentage = cleanEnergyCalculator.CalculateCleanEnergyPercentage(generationMix);

        Assert.Equal(0m, cleanEnergyPercentage);
    }
}
