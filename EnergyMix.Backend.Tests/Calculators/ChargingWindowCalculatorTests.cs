using EnergyMix.Backend.Calculators;
using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Tests.Helpers;
using EnergyMix.Backend.Utilities;
using Xunit;

namespace EnergyMix.Backend.Tests.Calculators;

public class ChargingWindowCalculatorTests
{
    [Fact]
    public void FindOptimalChargingWindow_ReturnsWindowWithHighestAverageCleanEnergy()
    {
        var startTime = new DateTimeOffset(2026, 6, 11, 22, 0, 0, TimeSpan.Zero);

        var generationIntervals = new List<GenerationIntervalDto>
        {
            GenerationTestDataBuilder.IntervalWithCleanEnergy(startTime, 10m),
            GenerationTestDataBuilder.IntervalWithCleanEnergy(startTime.AddMinutes(30), 20m),
            GenerationTestDataBuilder.IntervalWithCleanEnergy(startTime.AddMinutes(60), 60m),
            GenerationTestDataBuilder.IntervalWithCleanEnergy(startTime.AddMinutes(90), 70m),
            GenerationTestDataBuilder.IntervalWithCleanEnergy(startTime.AddMinutes(120), 80m),
            GenerationTestDataBuilder.IntervalWithCleanEnergy(startTime.AddMinutes(150), 90m),
            GenerationTestDataBuilder.IntervalWithCleanEnergy(startTime.AddMinutes(180), 30m)
        };

        var chargingWindowCalculator = new ChargingWindowCalculator(
            new CleanEnergyCalculator(),
            new EnergySourceShareCalculator());

        var result = chargingWindowCalculator.FindOptimalChargingWindow(generationIntervals, 2);

        Assert.Equal(startTime.AddMinutes(60), result.Start);
        Assert.Equal(startTime.AddMinutes(180), result.End);
        Assert.Equal(75m, result.AverageCleanEnergyPercentage);

        var sourcesByFuel = result.Sources.ToDictionary(
            source => source.Fuel,
            source => source.Percentage);

        Assert.Equal(2, result.Sources.Count);
        Assert.Equal(25m, sourcesByFuel["gas"]);
        Assert.Equal(75m, sourcesByFuel["wind"]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void FindOptimalChargingWindow_ThrowsException_WhenHoursAreOutsideAllowedRange(int hours)
    {
        var generationIntervals = new List<GenerationIntervalDto>
        {
            GenerationTestDataBuilder.IntervalWithCleanEnergy(
                new DateTimeOffset(2026, 6, 11, 0, 0, 0, TimeSpan.Zero),
                50m)
        };

        var chargingWindowCalculator = new ChargingWindowCalculator(
            new CleanEnergyCalculator(),
            new EnergySourceShareCalculator());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            chargingWindowCalculator.FindOptimalChargingWindow(generationIntervals, hours));
    }
}
