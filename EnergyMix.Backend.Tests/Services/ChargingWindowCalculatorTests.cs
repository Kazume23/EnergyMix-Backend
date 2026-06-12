using EnergyMix.Backend.Models;
using EnergyMix.Backend.Services;
using Xunit;

namespace EnergyMix.Backend.Tests.Services;

public class ChargingWindowCalculatorTests
{
    [Fact]
    public void FindOptimalChargingWindow_ReturnsWindowWithHighestAverageCleanEnergy()
    {
        var calculator = new ChargingWindowCalculator(new CleanEnergyCalculator());

        var startTime = new DateTimeOffset(2026, 6, 11, 22, 0, 0, TimeSpan.Zero);

        var generationIntervals = new List<GenerationInterval>
        {
            CreateInterval(startTime, 10m),
            CreateInterval(startTime.AddMinutes(30), 20m),
            CreateInterval(startTime.AddMinutes(60), 60m),
            CreateInterval(startTime.AddMinutes(90), 70m),
            CreateInterval(startTime.AddMinutes(120), 80m),
            CreateInterval(startTime.AddMinutes(150), 90m),
            CreateInterval(startTime.AddMinutes(180), 30m)
        };

        var result = calculator.FindOptimalChargingWindow(generationIntervals, 2);

        Assert.Equal(startTime.AddMinutes(60), result.Start);
        Assert.Equal(startTime.AddMinutes(180), result.End);
        Assert.Equal(75m, result.AverageCleanEnergyPercentage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void FindOptimalChargingWindow_ThrowsException_WhenHoursAreOutsideAllowedRange(int hours)
    {
        var calculator = new ChargingWindowCalculator(new CleanEnergyCalculator());

        var generationIntervals = new List<GenerationInterval>
        {
            CreateInterval(new DateTimeOffset(2026, 6, 11, 0, 0, 0, TimeSpan.Zero), 50m)
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            calculator.FindOptimalChargingWindow(generationIntervals, hours));
    }

    private static GenerationInterval CreateInterval(DateTimeOffset from, decimal cleanEnergyPercentage)
    {
        return new GenerationInterval
        {
            From = from,
            To = from.AddMinutes(30),
            GenerationMix = new List<GenerationMixItem>
            {
                new() { Fuel = "wind", Percentage = cleanEnergyPercentage },
                new() { Fuel = "gas", Percentage = 100m - cleanEnergyPercentage }
            }
        };
    }
}