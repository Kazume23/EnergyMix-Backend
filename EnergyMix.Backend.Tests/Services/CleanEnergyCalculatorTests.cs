using EnergyMix.Backend.Models;
using EnergyMix.Backend.Services;
using Xunit;

namespace EnergyMix.Backend.Tests.Services;

public class CleanEnergyCalculatorTests
{
    [Fact]
    public void CalculateCleanEnergyPercentage_ReturnsSumOfOnlyCleanEnergySources()
    {
        var calculator = new CleanEnergyCalculator();

        var generationMix = new List<GenerationMixItem>
        {
            new() { Fuel = "biomass", Percentage = 10m },
            new() { Fuel = "nuclear", Percentage = 20m },
            new() { Fuel = "hydro", Percentage = 5m },
            new() { Fuel = "wind", Percentage = 15m },
            new() { Fuel = "solar", Percentage = 7m },
            new() { Fuel = "gas", Percentage = 30m },
            new() { Fuel = "coal", Percentage = 13m }
        };

        var cleanEnergyPercentage = calculator.CalculateCleanEnergyPercentage(generationMix);

        Assert.Equal(57m, cleanEnergyPercentage);
    }

    [Fact]
    public void CalculateCleanEnergyPercentage_ReturnsZero_WhenThereAreNoCleanEnergySources()
    {
        var calculator = new CleanEnergyCalculator();

        var generationMix = new List<GenerationMixItem>
        {
            new() { Fuel = "gas", Percentage = 60m },
            new() { Fuel = "coal", Percentage = 20m },
            new() { Fuel = "imports", Percentage = 20m }
        };

        var cleanEnergyPercentage = calculator.CalculateCleanEnergyPercentage(generationMix);

        Assert.Equal(0m, cleanEnergyPercentage);
    }
}