using EnergyMix.Backend.Dtos.Requests;
using EnergyMix.Backend.Dtos.Responses;
using EnergyMix.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.Backend.Controllers;

[ApiController]
[Route("api/carbon")]
public class CarbonController : ControllerBase
{
    private readonly ICarbonService _carbonService;

    public CarbonController(ICarbonService carbonService)
    {
        _carbonService = carbonService;
    }

    [HttpGet("daily-mix")]
    [ProducesResponseType(typeof(List<DailyEnergyMixResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GetDailyMix(CancellationToken cancellationToken)
    {
        var dailyEnergyMix = await _carbonService.GetDailyMixAsync(cancellationToken);

        return Ok(dailyEnergyMix);
    }

    [HttpGet("optimal-charging-window")]
    [ProducesResponseType(typeof(OptimalChargingWindowResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> GetOptimalChargingWindow(
        [FromQuery] OptimalChargingWindowQueryDto query,
        CancellationToken cancellationToken)
    {
        var optimalChargingWindow = await _carbonService.GetOptimalChargingWindowAsync(
            query.Hours,
            cancellationToken);

        return Ok(optimalChargingWindow);
    }
}
