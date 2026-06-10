using Microsoft.AspNetCore.Mvc;

namespace EnergyMix.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarbonController : ControllerBase
{
    private readonly Services.CarbonIntensityService _carbonIntensityService;

    public CarbonController(Services.CarbonIntensityService carbonIntensityService)
    {
        _carbonIntensityService = carbonIntensityService;
    }

    [HttpGet("raw-generation")]
    public async Task<IActionResult> GetRawGeneration()
    {
        var rawGenerationMixJson = await _carbonIntensityService.GetRawGenerationMixAsync();

        return Content(rawGenerationMixJson, "application/json");
    }

}
