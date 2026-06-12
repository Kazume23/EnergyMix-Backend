using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Dtos.Responses;

namespace EnergyMix.Backend.Utilities;

public sealed class EnergyMixCalculator : IEnergyMixCalculator
{
    private readonly ICleanEnergyCalculator _cleanEnergyCalculator;
    private readonly IEnergySourceShareCalculator _energySourceShareCalculator;

    public EnergyMixCalculator(
        ICleanEnergyCalculator cleanEnergyCalculator,
        IEnergySourceShareCalculator energySourceShareCalculator)
    {
        _cleanEnergyCalculator = cleanEnergyCalculator;
        _energySourceShareCalculator = energySourceShareCalculator;
    }

    public List<DailyEnergyMixResponseDto> CalculateDailyEnergyMix(
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
                    Sources = _energySourceShareCalculator.CalculateAverageSourceShares(dailyIntervals),
                    CleanEnergyPercentage = decimal.Round(
                        dailyIntervals.Average(generationInterval =>
                            _cleanEnergyCalculator.CalculateCleanEnergyPercentage(generationInterval.GenerationMix)),
                        2)
                };
            })
            .ToList();
    }
}
