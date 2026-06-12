using EnergyMix.Backend.Calculators;
using EnergyMix.Backend.Dtos.CarbonApi;
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
            CreateInterval(startTime, 10m),
            CreateInterval(startTime.AddMinutes(30), 20m),
            CreateInterval(startTime.AddMinutes(60), 60m),
            CreateInterval(startTime.AddMinutes(90), 70m),
            CreateInterval(startTime.AddMinutes(120), 80m),
            CreateInterval(startTime.AddMinutes(150), 90m),
            CreateInterval(startTime.AddMinutes(180), 30m)
        };

        var result = ChargingWindowCalculator.FindOptimalChargingWindow(generationIntervals, 2);

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
            CreateInterval(new DateTimeOffset(2026, 6, 11, 0, 0, 0, TimeSpan.Zero), 50m)
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ChargingWindowCalculator.FindOptimalChargingWindow(generationIntervals, hours));
    }

    private static GenerationIntervalDto CreateInterval(DateTimeOffset from, decimal cleanEnergyPercentage)
    {
        return new GenerationIntervalDto
        {
            From = from,
            To = from.AddMinutes(30),
            GenerationMix = new List<GenerationMixItemDto>
            {
                new() { Fuel = "wind", Percentage = cleanEnergyPercentage },
                new() { Fuel = "gas", Percentage = 100m - cleanEnergyPercentage }
            }
        };
    }
}
