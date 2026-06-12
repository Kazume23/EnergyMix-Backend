using EnergyMix.Backend.Models;
using EnergyMix.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.Backend.Controllers;

[ApiController]
[Route("api/carbon")]
public class CarbonController : ControllerBase
{
    private readonly CarbonIntensityService _carbonIntensityService;
    private readonly EnergyMixCalculator _energyMixCalculator;
    private readonly ChargingWindowCalculator _chargingWindowCalculator;

    public CarbonController(CarbonIntensityService carbonIntensityService, EnergyMixCalculator energyMixCalculator, ChargingWindowCalculator chargingWindowCalculator)
    {
        _carbonIntensityService = carbonIntensityService;
        _energyMixCalculator = energyMixCalculator;
        _chargingWindowCalculator = chargingWindowCalculator;
    }

    [HttpGet("daily-mix")]
    public async Task<IActionResult> GetDailyMix()
    {
        var startDateUtc = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(3);

        var generationResponse = await _carbonIntensityService.GetGenerationAsync(
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

    [HttpGet("optimal-charging-window")]
    public async Task<IActionResult> GetOptimalChargingWindow([FromQuery] int hours)
    {
        var startDateUtc = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddDays(2);

        var generationResponse = await _carbonIntensityService.GetGenerationAsync(
            startDateUtc,
            endDateUtc);

        try
        {
            var optimalChargingWindow = _chargingWindowCalculator.FindOptimalChargingWindow(generationResponse.Data, hours);

            return Ok(optimalChargingWindow);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
