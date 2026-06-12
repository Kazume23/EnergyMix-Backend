using EnergyMix.Backend.Constants;
using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;
using EnergyMix.Backend.Exceptions;

namespace EnergyMix.Backend.Utilities;

public sealed class ChargingWindowCalculator : IChargingWindowCalculator
{
    private readonly ICleanEnergyCalculator _cleanEnergyCalculator;
    private readonly IEnergySourceShareCalculator _energySourceShareCalculator;

    public ChargingWindowCalculator(
        ICleanEnergyCalculator cleanEnergyCalculator,
        IEnergySourceShareCalculator energySourceShareCalculator)
    {
        _cleanEnergyCalculator = cleanEnergyCalculator;
        _energySourceShareCalculator = energySourceShareCalculator;
    }

    public OptimalChargingWindowResponseDto FindOptimalChargingWindow(
        IEnumerable<GenerationIntervalDto> generationIntervals,
        int hours)
    {
        if (hours < EnergyMixConstants.MinChargingHours || hours > EnergyMixConstants.MaxChargingHours)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be between 1 and 6.");
        }

        var requiredIntervalCount = hours * EnergyMixConstants.GenerationIntervalsPerHour;

        var sortedIntervals = generationIntervals
            .OrderBy(generationInterval => generationInterval.From)
            .ToList();

        if (sortedIntervals.Count < requiredIntervalCount)
        {
            throw new InsufficientGenerationDataException(
                "Not enough generation intervals to find an optimal charging window.");
        }

        var cleanEnergyPercentages = sortedIntervals
            .Select(generationInterval =>
                _cleanEnergyCalculator.CalculateCleanEnergyPercentage(generationInterval.GenerationMix))
            .ToList();

        var currentWindowCleanEnergyTotal = cleanEnergyPercentages
            .Take(requiredIntervalCount)
            .Sum();

        var bestStartIndex = 0;
        var bestCleanEnergyTotal = currentWindowCleanEnergyTotal;

        for (var endIndex = requiredIntervalCount; endIndex < cleanEnergyPercentages.Count; endIndex++)
        {
            currentWindowCleanEnergyTotal += cleanEnergyPercentages[endIndex];
            currentWindowCleanEnergyTotal -= cleanEnergyPercentages[endIndex - requiredIntervalCount];

            if (currentWindowCleanEnergyTotal > bestCleanEnergyTotal)
            {
                bestCleanEnergyTotal = currentWindowCleanEnergyTotal;
                bestStartIndex = endIndex - requiredIntervalCount + 1;
            }
        }

        var bestWindowIntervals = sortedIntervals
            .Skip(bestStartIndex)
            .Take(requiredIntervalCount)
            .ToList();

        return new OptimalChargingWindowResponseDto
        {
            Start = bestWindowIntervals.First().From,
            End = bestWindowIntervals.Last().To,
            AverageCleanEnergyPercentage = decimal.Round(bestCleanEnergyTotal / requiredIntervalCount, 2),
            Sources = _energySourceShareCalculator.CalculateAverageSourceShares(bestWindowIntervals)
        };
    }
}
