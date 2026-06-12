using EnergyMix.Backend.Calculators;
using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Tests.Helpers;
using Xunit;

namespace EnergyMix.Backend.Tests.Calculators;

public class EnergyMixCalculatorTests
{
    [Fact]
    public void CalculateDailyEnergyMix_GroupsIntervalsByDateAndCalculatesDailyAverages()
    {
        var generationIntervals = new List<GenerationIntervalDto>
        {
            GenerationTestDataBuilder.Interval(
                new DateTimeOffset(2026, 6, 11, 0, 0, 0, TimeSpan.Zero),
                GenerationTestDataBuilder.MixItem("biomass", 10m),
                GenerationTestDataBuilder.MixItem("wind", 20m),
                GenerationTestDataBuilder.MixItem("gas", 70m)),
            GenerationTestDataBuilder.Interval(
                new DateTimeOffset(2026, 6, 11, 0, 30, 0, TimeSpan.Zero),
                GenerationTestDataBuilder.MixItem("biomass", 30m),
                GenerationTestDataBuilder.MixItem("wind", 10m),
                GenerationTestDataBuilder.MixItem("gas", 60m)),
            GenerationTestDataBuilder.Interval(
                new DateTimeOffset(2026, 6, 12, 0, 0, 0, TimeSpan.Zero),
                GenerationTestDataBuilder.MixItem("nuclear", 50m),
                GenerationTestDataBuilder.MixItem("gas", 50m))
        };

        var result = EnergyMixCalculator.CalculateDailyEnergyMix(generationIntervals);

        Assert.Equal(2, result.Count);

        var firstDay = result[0];

        Assert.Equal(new DateOnly(2026, 6, 11), firstDay.Date);
        Assert.Equal(35m, firstDay.CleanEnergyPercentage);
        Assert.Contains(firstDay.Sources, source => source.Fuel == "biomass" && source.Percentage == 20m);
        Assert.Contains(firstDay.Sources, source => source.Fuel == "wind" && source.Percentage == 15m);
        Assert.Contains(firstDay.Sources, source => source.Fuel == "gas" && source.Percentage == 65m);

        var secondDay = result[1];

        Assert.Equal(new DateOnly(2026, 6, 12), secondDay.Date);
        Assert.Equal(50m, secondDay.CleanEnergyPercentage);
        Assert.Contains(secondDay.Sources, source => source.Fuel == "nuclear" && source.Percentage == 50m);
        Assert.Contains(secondDay.Sources, source => source.Fuel == "gas" && source.Percentage == 50m);
    }
}
