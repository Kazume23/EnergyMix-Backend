using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;
using EnergyMix.Backend.Utilities;

namespace EnergyMix.Backend.Calculators;

public static class ChargingWindowCalculator
{
    public static OptimalChargingWindowResponseDto FindOptimalChargingWindow(
        IEnumerable<GenerationIntervalDto> generationIntervals,
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
        List<GenerationIntervalDto> bestWindowIntervals = [];

        for (var startIndex = 0; startIndex <= sortedIntervals.Count - requiredIntervalCount; startIndex++)
        {
            var currentWindowIntervals = sortedIntervals
                .Skip(startIndex)
                .Take(requiredIntervalCount)
                .ToList();

            var currentWindowsAverageCleanEnergy = currentWindowIntervals
                .Average(generationInterval =>
                    CleanEnergyCalculator.CalculateCleanEnergyPercentage(generationInterval.GenerationMix));

            if (currentWindowsAverageCleanEnergy > bestCleanEnergyPercentage)
            {
                bestCleanEnergyPercentage = currentWindowsAverageCleanEnergy;
                bestWindowIntervals = currentWindowIntervals;
            }
        }

        return new OptimalChargingWindowResponseDto
        {
            Start = bestWindowIntervals.First().From,
            End = bestWindowIntervals.Last().To,
            AverageCleanEnergyPercentage = decimal.Round(bestCleanEnergyPercentage, 2),
            Sources = EnergySourceShareCalculator.CalculateAverageSourceShares(bestWindowIntervals)
        };
    }
}
