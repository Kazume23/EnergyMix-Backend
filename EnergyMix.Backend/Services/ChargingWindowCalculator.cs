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

        var sortedIntervals = new List<GenerationInterval>(generationIntervals);
        sortedIntervals.Sort((firstInterval, secondInterval) => firstInterval.From.CompareTo(secondInterval.From));

        if (sortedIntervals.Count < requiredIntervalCount)
        {
            throw new InvalidOperationException("Not enough generation intervals to find an optimal charging window.");
        }

        decimal bestCleanEnergyPercentage = -1m;
        DateTimeOffset bestStartTime = default;
        DateTimeOffset bestEndTime = default;

        for (var startIndex = 0; startIndex <= sortedIntervals.Count - requiredIntervalCount; startIndex++)
        {
            decimal currentWindowsCleanEnergyTotal = 0m;

            for (var intervalOffset = 0; intervalOffset < requiredIntervalCount; intervalOffset++)
            {
                var currentInterval = sortedIntervals[startIndex + intervalOffset];

                currentWindowsCleanEnergyTotal += _cleanEnergyCalculator.CalculateCleanEnergyPercentage(currentInterval.GenerationMix);
            }

            var currentWindowsAverageCleanEnergy = currentWindowsCleanEnergyTotal / requiredIntervalCount;

            if (currentWindowsAverageCleanEnergy > bestCleanEnergyPercentage)
            {
                var firstIntervalInWindow = sortedIntervals[startIndex];
                var lastIntervalInWindow = sortedIntervals[startIndex + requiredIntervalCount - 1];

                bestCleanEnergyPercentage = currentWindowsAverageCleanEnergy;
                bestStartTime = firstIntervalInWindow.From;
                bestEndTime = lastIntervalInWindow.To;
            }
        }

        return new OptimalChargingWindowResponse
        {
            Start = bestStartTime,
            End = bestEndTime,
            AverageCleanEnergyPercentage = decimal.Round(bestCleanEnergyPercentage, 2)
        };
    }

}
