using EnergyMix.Backend.Controllers;
using EnergyMix.Backend.Dtos.Requests;
using EnergyMix.Backend.Dtos.Responses;
using EnergyMix.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace EnergyMix.Backend.Tests.Controllers;

public class CarbonControllerTests
{
    [Fact]
    public async Task GetDailyMix_ReturnsOkWithDailyEnergyMix()
    {
        var expectedResponse = new List<DailyEnergyMixResponseDto>
        {
            new()
            {
                Date = new DateOnly(2026, 6, 14),
                CleanEnergyPercentage = 50m
            }
        };

        var controller = new CarbonController(new FakeCarbonService(expectedResponse));

        var result = await controller.GetDailyMix();

        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Same(expectedResponse, okResult.Value);
    }

    [Fact]
    public async Task GetOptimalChargingWindow_ReturnsOkWithOptimalWindow()
    {
        var expectedResponse = new OptimalChargingWindowResponseDto
        {
            Start = new DateTimeOffset(2026, 6, 14, 1, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 6, 14, 2, 0, 0, TimeSpan.Zero),
            AverageCleanEnergyPercentage = 60m
        };

        var controller = new CarbonController(new FakeCarbonService(expectedResponse));

        var result = await controller.GetOptimalChargingWindow(
            new OptimalChargingWindowQueryDto { Hours = 1 });

        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Same(expectedResponse, okResult.Value);
    }

    private sealed class FakeCarbonService : ICarbonService
    {
        private readonly List<DailyEnergyMixResponseDto> _dailyEnergyMixResponse;
        private readonly OptimalChargingWindowResponseDto _optimalChargingWindowResponse;

        public FakeCarbonService(List<DailyEnergyMixResponseDto> dailyEnergyMixResponse)
        {
            _dailyEnergyMixResponse = dailyEnergyMixResponse;
            _optimalChargingWindowResponse = new OptimalChargingWindowResponseDto();
        }

        public FakeCarbonService(OptimalChargingWindowResponseDto optimalChargingWindowResponse)
        {
            _dailyEnergyMixResponse = [];
            _optimalChargingWindowResponse = optimalChargingWindowResponse;
        }

        public Task<List<DailyEnergyMixResponseDto>> GetDailyMixAsync()
        {
            return Task.FromResult(_dailyEnergyMixResponse);
        }

        public Task<OptimalChargingWindowResponseDto> GetOptimalChargingWindowAsync(int hours)
        {
            return Task.FromResult(_optimalChargingWindowResponse);
        }
    }
}
