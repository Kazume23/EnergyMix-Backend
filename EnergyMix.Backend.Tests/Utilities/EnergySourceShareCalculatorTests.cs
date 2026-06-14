using EnergyMix.Backend.Tests.Helpers;
using EnergyMix.Backend.Utilities;
using Xunit;

namespace EnergyMix.Backend.Tests.Utilities;

public class EnergySourceShareCalculatorTests
{
    [Fact]
    public void CalculateAverageSourceShares_ReturnsAveragePercentageForEachFuel()
    {
        var startTime = new DateTimeOffset(2026, 6, 14, 0, 0, 0, TimeSpan.Zero);
        var generationIntervals = new[]
        {
            GenerationTestDataBuilder.Interval(
                startTime,
                GenerationTestDataBuilder.MixItem("wind", 20m),
                GenerationTestDataBuilder.MixItem("gas", 80m)),
            GenerationTestDataBuilder.Interval(
                startTime.AddMinutes(30),
                GenerationTestDataBuilder.MixItem("wind", 40m),
                GenerationTestDataBuilder.MixItem("gas", 60m))
        };

        var energySourceShareCalculator = new EnergySourceShareCalculator();

        var result = energySourceShareCalculator.CalculateAverageSourceShares(generationIntervals);

        Assert.Contains(result, source => source.Fuel == "gas" && source.Percentage == 70m);
        Assert.Contains(result, source => source.Fuel == "wind" && source.Percentage == 30m);
    }
}
