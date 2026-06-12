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
        var intervalsByDate = new SortedDictionary<DateOnly, List<GenerationInterval>>();

        foreach (var generationInterval in generationIntervals)
        {
            var intervalDate = DateOnly.FromDateTime(generationInterval.From.UtcDateTime);

            if (!intervalsByDate.ContainsKey(intervalDate))
            {
                intervalsByDate[intervalDate] = [];
            }

            intervalsByDate[intervalDate].Add(generationInterval);
        }

        var dailyEnergyMixResponses = new List<DailyEnergyMixResponse>();

        foreach (var dailyIntervalsGroup in intervalsByDate)
        {
            var sourceTotals = new SortedDictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            var sourceCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var cleanEnergyTotal = 0m;

            foreach (var generationInterval in dailyIntervalsGroup.Value)
            {
                cleanEnergyTotal += _cleanEnergyCalculator.CalculateCleanEnergyPercentage(generationInterval.GenerationMix);

                foreach (var generationMixItem in generationInterval.GenerationMix)
                {
                    if (!sourceTotals.ContainsKey(generationMixItem.Fuel))
                    {
                        sourceTotals[generationMixItem.Fuel] = 0m;
                        sourceCounts[generationMixItem.Fuel] = 0;
                    }

                    sourceTotals[generationMixItem.Fuel] += generationMixItem.Percentage;
                    sourceCounts[generationMixItem.Fuel]++;
                }
            }

            var sourceResponses = new List<EnergySourceShareResponse>();

            foreach (var sourceTotal in sourceTotals)
            {
                var averagePercentage = sourceTotal.Value / sourceCounts[sourceTotal.Key];

                sourceResponses.Add(new EnergySourceShareResponse
                {
                    Fuel = sourceTotal.Key,
                    Percentage = decimal.Round(averagePercentage, 2)
                });
            }

            var averageCleanEnergyPercentage = cleanEnergyTotal / dailyIntervalsGroup.Value.Count;

            dailyEnergyMixResponses.Add(new DailyEnergyMixResponse
            {
                Date = dailyIntervalsGroup.Key,
                Sources = sourceResponses,
                CleanEnergyPercentage = decimal.Round(averageCleanEnergyPercentage, 2)
            });
        }

        return dailyEnergyMixResponses;
    }
}