using EnergyMix.Backend.Models;

namespace EnergyMix.Backend.Services;

public sealed class ChargingWindowCalculator
{
    private readonly CleanEnergyCalculator _cleanEnergyCalculator;

    public ChargingWindowCalculator(CleanEnergyCalculator cleanEnergyCalculator)
    {
        _cleanEnergyCalculator = cleanEnergyCalculator;
    }

    public OptimalChargingWindowResponse FindOptimalChargingWindow(
        IEnumerable<GenerationInterval> generationIntervals,
        int hours)
    {
        if (hours < 1 || hours > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be between 1 and 6.");
        }

        var requiredIntervalCount = hours * 2;

        var sortedIntervals = generationIntervals
            .OrderBy(generationInterval => generationInterval.From)
            .ToList();

        if (sortedIntervals.Count < requiredIntervalCount)
        {
            throw new InvalidOperationException("Not enough generation intervals to find an optimal charging window.");
        }

        decimal bestCleanEnergyPercentage = -1m;
        List<GenerationInterval> bestWindowIntervals = [];

        for (var startIndex = 0; startIndex <= sortedIntervals.Count - requiredIntervalCount; startIndex++)
        {
            var currentWindowIntervals = sortedIntervals
                .Skip(startIndex)
                .Take(requiredIntervalCount)
                .ToList();

            var currentWindowsAverageCleanEnergy = currentWindowIntervals
                .Average(generationInterval =>
                    _cleanEnergyCalculator.CalculateCleanEnergyPercentage(generationInterval.GenerationMix));

            if (currentWindowsAverageCleanEnergy > bestCleanEnergyPercentage)
            {
                bestCleanEnergyPercentage = currentWindowsAverageCleanEnergy;
                bestWindowIntervals = currentWindowIntervals;
            }
        }

        return new OptimalChargingWindowResponse
        {
            Start = bestWindowIntervals.First().From,
            End = bestWindowIntervals.Last().To,
            AverageCleanEnergyPercentage = decimal.Round(bestCleanEnergyPercentage, 2),
            Sources = CalculateAverageSourceShares(bestWindowIntervals)
        };
    }

    private static List<EnergySourceShareResponse> CalculateAverageSourceShares(
        IEnumerable<GenerationInterval> generationIntervals)
    {
        return generationIntervals
            .SelectMany(generationInterval => generationInterval.GenerationMix)
            .GroupBy(generationMixItem => generationMixItem.Fuel, StringComparer.OrdinalIgnoreCase)
            .OrderBy(sourceGroup => sourceGroup.Key, StringComparer.OrdinalIgnoreCase)
            .Select(sourceGroup => new EnergySourceShareResponse
            {
                Fuel = sourceGroup.Key,
                Percentage = decimal.Round(sourceGroup.Average(generationMixItem => generationMixItem.Percentage), 2)
            })
            .ToList();
    }
}
