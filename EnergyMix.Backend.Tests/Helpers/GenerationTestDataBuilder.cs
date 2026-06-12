using EnergyMix.Backend.Dtos.CarbonApi;

namespace EnergyMix.Backend.Tests.Helpers;

internal static class GenerationTestDataBuilder
{
    public static GenerationMixItemDto MixItem(string fuel, decimal percentage)
    {
        return new GenerationMixItemDto
        {
            Fuel = fuel,
            Percentage = percentage
        };
    }

    public static GenerationIntervalDto Interval(
        DateTimeOffset from,
        params GenerationMixItemDto[] generationMix)
    {
        return new GenerationIntervalDto
        {
            From = from,
            To = from.AddMinutes(30),
            GenerationMix = generationMix.ToList()
        };
    }

    public static GenerationIntervalDto IntervalWithCleanEnergy(
        DateTimeOffset from,
        decimal cleanEnergyPercentage)
    {
        return Interval(
            from,
            MixItem("wind", cleanEnergyPercentage),
            MixItem("gas", 100m - cleanEnergyPercentage));
    }
}
