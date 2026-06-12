using EnergyMix.Backend.Models;

namespace EnergyMix.Backend.Services;

public sealed class EnergyMixCalculator
{
    private readonly CleanEnergyCalculator _cleanEnergyCalculator;

    public EnergyMixCalculator(CleanEnergyCalculator cleanEnergyCalculator)
    {
        _cleanEnergyCalculator = cleanEnergyCalculator;
    }

    public List<DailyEnergyMixResponse> CalculateDailyEnergyMix(IEnumerable<GenerationInterval> generationIntervals)
    {
        return generationIntervals
            .GroupBy(generationInterval => DateOnly.FromDateTime(generationInterval.From.UtcDateTime))
            .OrderBy(dailyIntervalsGroup => dailyIntervalsGroup.Key)
            .Select(dailyIntervalsGroup =>
            {
                var dailyIntervals = dailyIntervalsGroup.ToList();

                return new DailyEnergyMixResponse
                {
                    Date = dailyIntervalsGroup.Key,
                    Sources = CalculateAverageSourceShares(dailyIntervals),
                    CleanEnergyPercentage = decimal.Round(
                        dailyIntervals.Average(generationInterval =>
                            _cleanEnergyCalculator.CalculateCleanEnergyPercentage(generationInterval.GenerationMix)),
                        2)
                };
            })
            .ToList();
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
