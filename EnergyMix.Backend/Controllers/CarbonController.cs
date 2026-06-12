using EnergyMix.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.Backend.Controllers;

[ApiController]
[Route("api/carbon")]
public class CarbonController : ControllerBase
{
    private readonly CarbonService _carbonService;

    public CarbonController(CarbonService carbonService)
    {
        _carbonService = carbonService;
    }

    [HttpGet("daily-mix")]
    public async Task<IActionResult> GetDailyMix()
    {
        var dailyEnergyMix = await _carbonService.GetDailyMixAsync();

        return Ok(dailyEnergyMix);
    }

    [HttpGet("optimal-charging-window")]
    public async Task<IActionResult> GetOptimalChargingWindow([FromQuery] int hours)
    {
        try
        {
            var optimalChargingWindow = await _carbonService.GetOptimalChargingWindowAsync(hours);

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
