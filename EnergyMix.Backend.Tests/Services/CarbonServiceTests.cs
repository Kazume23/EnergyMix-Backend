using EnergyMix.Backend.Clients;
using EnergyMix.Backend.Dtos.CarbonApi;
using EnergyMix.Backend.Services;
using EnergyMix.Backend.Tests.Helpers;
using EnergyMix.Backend.Utilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EnergyMix.Backend.Tests.Services;

public class CarbonServiceTests
{
    [Fact]
    public async Task GetDailyMixAsync_FetchesThreeDaysOfGenerationData()
    {
        var currentTimeUtc = new DateTimeOffset(2026, 6, 14, 10, 0, 0, TimeSpan.Zero);
        var carbonIntensityClient = new FakeCarbonIntensityClient(CreateGenerationResponse(currentTimeUtc));
        var carbonService = CreateCarbonService(carbonIntensityClient, currentTimeUtc);

        await carbonService.GetDailyMixAsync();

        var request = Assert.Single(carbonIntensityClient.Requests);

        Assert.Equal(new DateTimeOffset(2026, 6, 14, 0, 0, 0, TimeSpan.Zero), request.StartDateUtc);
        Assert.Equal(new DateTimeOffset(2026, 6, 17, 0, 0, 0, TimeSpan.Zero), request.EndDateUtc);
    }

    [Fact]
    public async Task GetOptimalChargingWindowAsync_FetchesTwoDaysOfGenerationData()
    {
        var currentTimeUtc = new DateTimeOffset(2026, 6, 14, 10, 0, 0, TimeSpan.Zero);
        var carbonIntensityClient = new FakeCarbonIntensityClient(CreateGenerationResponse(currentTimeUtc));
        var carbonService = CreateCarbonService(carbonIntensityClient, currentTimeUtc);

        await carbonService.GetOptimalChargingWindowAsync(1);

        var request = Assert.Single(carbonIntensityClient.Requests);

        Assert.Equal(new DateTimeOffset(2026, 6, 14, 0, 0, 0, TimeSpan.Zero), request.StartDateUtc);
        Assert.Equal(new DateTimeOffset(2026, 6, 16, 0, 0, 0, TimeSpan.Zero), request.EndDateUtc);
    }

    [Fact]
    public async Task GetDailyMixAsync_UsesCachedGenerationData()
    {
        var currentTimeUtc = new DateTimeOffset(2026, 6, 14, 10, 0, 0, TimeSpan.Zero);
        var carbonIntensityClient = new FakeCarbonIntensityClient(CreateGenerationResponse(currentTimeUtc));
        var carbonService = CreateCarbonService(carbonIntensityClient, currentTimeUtc);

        await carbonService.GetDailyMixAsync();
        await carbonService.GetDailyMixAsync();

        Assert.Single(carbonIntensityClient.Requests);
    }

    private static CarbonService CreateCarbonService(
        ICarbonIntensityClient carbonIntensityClient,
        DateTimeOffset currentTimeUtc)
    {
        var cleanEnergyCalculator = new CleanEnergyCalculator();
        var energySourceShareCalculator = new EnergySourceShareCalculator();

        return new CarbonService(
            carbonIntensityClient,
            new ChargingWindowCalculator(cleanEnergyCalculator, energySourceShareCalculator),
            new EnergyMixCalculator(cleanEnergyCalculator, energySourceShareCalculator),
            NullLogger<CarbonService>.Instance,
            new MemoryCache(new MemoryCacheOptions()),
            new TestTimeProvider(currentTimeUtc));
    }

    private static CarbonGenerationResponseDto CreateGenerationResponse(DateTimeOffset currentTimeUtc)
    {
        var dayStartUtc = new DateTimeOffset(currentTimeUtc.UtcDateTime.Date, TimeSpan.Zero);

        return new CarbonGenerationResponseDto
        {
            Data = new List<GenerationIntervalDto>
            {
                GenerationTestDataBuilder.IntervalWithCleanEnergy(dayStartUtc, 40m),
                GenerationTestDataBuilder.IntervalWithCleanEnergy(dayStartUtc.AddMinutes(30), 60m),
                GenerationTestDataBuilder.IntervalWithCleanEnergy(dayStartUtc.AddMinutes(60), 80m)
            }
        };
    }

    private sealed class FakeCarbonIntensityClient : ICarbonIntensityClient
    {
        private readonly CarbonGenerationResponseDto _generationResponse;

        public FakeCarbonIntensityClient(CarbonGenerationResponseDto generationResponse)
        {
            _generationResponse = generationResponse;
        }

        public List<(DateTimeOffset StartDateUtc, DateTimeOffset EndDateUtc)> Requests { get; } = [];

        public Task<CarbonGenerationResponseDto> GetGenerationAsync(
            DateTimeOffset startDateUtc,
            DateTimeOffset endDateUtc,
            CancellationToken cancellationToken = default)
        {
            Requests.Add((startDateUtc, endDateUtc));

            return Task.FromResult(_generationResponse);
        }
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}
