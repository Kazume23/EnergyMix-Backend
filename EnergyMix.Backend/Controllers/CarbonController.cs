using EnergyMix.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarbonController : ControllerBase
{
    private readonly CarbonIntensityService _carbonIntensityService;

    public CarbonController(CarbonIntensityService carbonIntensityService)
    {
        _carbonIntensityService = carbonIntensityService;
    }

    [HttpGet("raw-generation")]
    public async Task<IActionResult> GetRawGeneration()
    {
        var startDateUtc = DateTimeOffset.UtcNow.Date;
        var endDateUtc = startDateUtc.AddDays(1);

        var generationResponse = await _carbonIntensityService.GetRawGenerationMixAsync(
            startDateUtc,
            endDateUtc);

        return Ok(generationResponse);
    }
}