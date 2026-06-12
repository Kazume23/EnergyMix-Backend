using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;
using EnergyMix.Backend.Utilities;

namespace EnergyMix.Backend.Calculators;

public static class EnergyMixCalculator
{
    public static List<DailyEnergyMixResponseDto> CalculateDailyEnergyMix(
        IEnumerable<GenerationIntervalDto> generationIntervals)
    {
        return generationIntervals
            .GroupBy(generationInterval => DateOnly.FromDateTime(generationInterval.From.UtcDateTime))
            .OrderBy(dailyIntervalsGroup => dailyIntervalsGroup.Key)
            .Select(dailyIntervalsGroup =>
            {
                var dailyIntervals = dailyIntervalsGroup.ToList();

                return new DailyEnergyMixResponseDto
                {
                    Date = dailyIntervalsGroup.Key,
                    Sources = EnergySourceShareCalculator.CalculateAverageSourceShares(dailyIntervals),
                    CleanEnergyPercentage = decimal.Round(
                        dailyIntervals.Average(generationInterval =>
                            CleanEnergyCalculator.CalculateCleanEnergyPercentage(generationInterval.GenerationMix)),
                        2)
                };
            })
            .ToList();
    }
}
