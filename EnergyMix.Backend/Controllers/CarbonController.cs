using EnergyMix.Backend.Models;
using EnergyMix.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarbonController : ControllerBase
{
    private readonly CarbonIntensityService _carbonIntensityService;
    private readonly EnergyMixCalculator _energyMixCalculator;

    public CarbonController(CarbonIntensityService carbonIntensityService, EnergyMixCalculator energyMixCalculator)
    {
        _carbonIntensityService = carbonIntensityService;
        _energyMixCalculator = energyMixCalculator;
    }

    [HttpGet("raw-generation")]
    public async Task<IActionResult> GetRawGeneration()
    {
        var startDateUtc = DateTimeOffset.UtcNow.Date;
        var endDateUtc = startDateUtc.AddDays(1);

        var generationResponse = await _carbonIntensityService.GetGenerationMixAsync(
            startDateUtc,
            endDateUtc);

        return Ok(generationResponse);
    }
    [HttpGet("daily-mix")]
    public async Task<IActionResult> GetDailyMix()
    {
        var startDateUtc = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(3);

        var generationResponse = await _carbonIntensityService.GetGenerationMixAsync(
            startDateUtc,
            endDateUtc);

        var requestedIntervals = new List<GenerationInterval>();

        foreach (var generationInterval in generationResponse.Data)
        {
            if (generationInterval.From >= startDateUtc && generationInterval.To <= endDateUtc)
            {
                requestedIntervals.Add(generationInterval);
            }
        }

        var dailyEnergyMix = _energyMixCalculator.CalculateDailyEnergyMix(requestedIntervals);

        return Ok(dailyEnergyMix);
    }
}